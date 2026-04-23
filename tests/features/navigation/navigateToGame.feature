Feature: Navigate to game
    Background:
        Given I am on the home page
        When I click the "Create Game" button
        When I type "name2" into the game name field
        And I click the "Create Game" button
    
    Scenario: User enters the lobby
        And I click the "Start" button
        Then I should be taken to the "game" page


        