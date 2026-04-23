Feature: Navigate to lobby

    Scenario: User navigates to lobby from home page
        Given I am on the home page
        When I click the "Lobby" button
        And I click the "Confirm" button
        Then I should be taken to the lobby page