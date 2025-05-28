Feature: Check calculating rank and similarity of the 
    Scenario: First text evaluation then re-evaluation
        Given I open web application 
        When I select region "Russia"
        And I enter text "a1b2"
        And I click the "Submit" button
        Then I should see rank "0,5" and similarity "0"

        Given I go to main page
        When I select region "Russia"
        And I enter text "a1b2"
        And I click the "Submit" button
        Then I should see rank "0,5" and similarity "1"

    Scenario: Evaluation in different region
        Given I open web application 
        When I select region "Germany"
        And I enter text "a1b2"
        And I click the "Submit" button
        Then I should see rank "0,5" and similarity "0"