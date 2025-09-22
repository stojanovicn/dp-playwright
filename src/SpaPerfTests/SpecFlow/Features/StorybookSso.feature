Feature: Storybook SSO Login
  As a user
  I want to login via SSO to Storybook docs
  So that I can view the Introduction page

  @external
  Scenario: Login to Storybook docs through SSO
    Given I open Storybook intro docs
    Then I should be redirected to SSO
    When I login via SSO
    Then I arrive on Storybook intro
    And I open all sidebar docs


