using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace SpaPerfTests
{
	public static class CdpHelper
	{
		public sealed record PerformanceMetrics(
			double JsHeapUsedSizeBytes,
			double JsHeapTotalSizeBytes,
			double TaskDurationMs,
			IReadOnlyDictionary<string, double> Raw);

		public static async Task<PerformanceMetrics> GetPerformanceMetricsAsync(IPage page)
		{
			var session = await page.Context.NewCDPSessionAsync(page);
			// Ensure performance domain is enabled to get non-zero values
			try { await session.SendAsync("Performance.enable"); } catch { /* ignore */ }
			var result = await session.SendAsync("Performance.getMetrics");
			var json = JsonSerializer.Serialize(result);
			using var doc = JsonDocument.Parse(json);
			var dict = new Dictionary<string, double>(StringComparer.Ordinal);
			if (doc.RootElement.TryGetProperty("metrics", out var metricsEl) && metricsEl.ValueKind == JsonValueKind.Array)
			{
				foreach (var m in metricsEl.EnumerateArray())
				{
					var name = m.TryGetProperty("name", out var n) ? n.GetString() : null;
					var value = m.TryGetProperty("value", out var v) ? v.GetDouble() : 0d;
					if (!string.IsNullOrEmpty(name))
					{
						dict[name!] = value;
					}
				}
			}

			dict.TryGetValue("JSHeapUsedSize", out var used);
			dict.TryGetValue("JSHeapTotalSize", out var total);
			dict.TryGetValue("TaskDuration", out var taskDurationSeconds);

			// Fallback to window.performance.memory if CDP returns zeros
			if (used <= 0 || total <= 0)
			{
				try
				{
					var mem = await page.EvaluateAsync<JsonElement?>(
						"(() => (performance && performance.memory) ? {used: performance.memory.usedJSHeapSize, total: performance.memory.totalJSHeapSize} : null)()"
					);
					if (mem.HasValue)
					{
						if (mem.Value.TryGetProperty("used", out var u)) used = u.GetDouble();
						if (mem.Value.TryGetProperty("total", out var t)) total = t.GetDouble();
					}
				}
				catch { /* ignore */ }
			}

			var taskDurationMs = taskDurationSeconds * 1000.0;

			return new PerformanceMetrics(
				JsHeapUsedSizeBytes: used,
				JsHeapTotalSizeBytes: total,
				TaskDurationMs: taskDurationMs,
				Raw: dict);
		}

		public static async Task SetCpuThrottlingRateAsync(IPage page, double rate)
		{
			var session = await page.Context.NewCDPSessionAsync(page);
			await session.SendAsync("Emulation.setCPUThrottlingRate", new Dictionary<string, object>
			{
				["rate"] = rate
			});
		}

		public static async Task ForceGcAsync(IPage page)
		{
			_ = await page.EvaluateAsync("globalThis.gc && globalThis.gc()");
			await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
		}
	}
}


