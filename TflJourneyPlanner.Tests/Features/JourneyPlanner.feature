Feature: Journey Planner
  Plan a journey on the TfL website with the "Plan a journey" widget,
  also known as the "Journey Planner".

  Background:
    Given the Journey Planner is open

  @smoke
  Scenario: The Journey Planner loads
    Then the Journey Planner is shown

  Rule: A journey can only be planned between valid locations

    @smoke
    Scenario: Plan a journey between two stations chosen from autocomplete suggestions
      When I type "Leicester Sq" in From and choose "Leicester Square Underground Station"
      And I type "Covent Gard" in To and choose "Covent Garden Underground Station"
      And I plan my journey
      Then the walking time is shown in minutes
      And the cycling time is shown in minutes

    @negative
    Scenario Outline: No results are shown when an invalid journey is planned (<case>)
      When I enter "<from>" as the From location
      And I enter "<to>" as the To location
      And I plan my journey
      Then "Journey planner could not find any results to your search. Please try again" is shown
      And no journey options are shown

      Examples:
        | case         | from                                 | to                                |
        | invalid From | #####                                | Covent Garden Underground Station |
        | invalid To   | Leicester Square Underground Station | @@@@@                             |
        | both invalid | @@@@@                                | #####                             |

    @negative
    Scenario: A journey cannot be planned if no locations are entered
      When I plan my journey without entering any locations
      Then "The From field is required." is shown
      And "The To field is required." is shown
      And no journey options are shown

  Rule: Planned journeys can be refined with preferences

    # Results depend on when you travel: late at night, "least walking" returns night buses,
    # and weekend engineering works for the Piccadilly line upgrade may change the routes.
    # These scenarios travel at midday on a weekday.
    Background:
      Given I am leaving at "12:00" on the next weekday
      And I type "Leicester Sq" in From and choose "Leicester Square Underground Station"
      And I type "Covent Gard" in To and choose "Covent Garden Underground Station"
      And I plan my journey

    Scenario: Show routes with least walking
      When I edit preferences to show "Routes with least walking"
      And I update my journey
      Then the results show routes with least walking
      And every journey option shows a journey time in minutes

    Scenario: View complete access information for a station
      Given I have edited preferences to show "Routes with least walking"
      When I view the details of the first journey option
      Then the access information at "Covent Garden Underground Station" is:
        | Access information |
        | Up stairs          |
        | Up lift            |
        | Level walkway      |
