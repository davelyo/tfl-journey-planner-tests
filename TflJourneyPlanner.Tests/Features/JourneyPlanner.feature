Feature: Journey Planner
  Plan a journey on the TfL website with the "Journey Planner" widget.

  @smoke
  Scenario: The Journey Planner loads
    When I open the Journey Planner
    Then the Journey Planner is shown
