Feature: Navigate to lobby

    Scenario: User navigates to lobby from home page
        Given I am on the home page
        When I click the "Create Game" button
        When I type "name" into the game name field
        And I click the "Create Game" button
        Then I should be taken to the lobby page