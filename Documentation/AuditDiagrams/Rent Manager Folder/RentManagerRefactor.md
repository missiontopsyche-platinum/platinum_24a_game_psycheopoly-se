\# Rent Manager Folder Refactor



\*\*Author(s):\*\* Dakota Zadroga

\*\*Date Created:\*\* 03/21/2026

\*\*Last Updated:\*\* 03/21/2026

\*\*Version:\*\* 1.0



\---



\## Purpose

This document explains the changes that have been made to the rent manager folder during refactoring. 



\## Scope

This document applies to the Rent Manager Folder



\## Related Documents

\- https://github.com/MissionToPsyche-Platinum/platinum\_24a\_game\_psycheopoly-se/blob/US783/Documentation/AuditDiagrams/Rent%20Manager%20Folder/Rent%20Manager%20Integration%20Audit.pdf



\## Notes

No additional notes



\---



\## Goals of Refactor

* Remove unnecessary abstract lays like the strategy pattern and adapters
* Simplify the rent calculation logic
* Improve overall readability and debugging
* Refactor the Unit Tests
* Reduce the coupling between systems



\## Major Changes



\### 1. Removed Strategy Pattern



\*\*Deleted\*\*

* IRentStrategy
* StandardRentStrategy



\*\*Replaced With:\*\*

* RentCalculator



\*\*Reason\*\*

The strategy pattern is not necessary since only one strategy was implemented.  A static calculator creates a simpler way to implement the logic. 





\### 2. Created RentCalculator.cs



A static class that is responsible for computing rent based on

* Tile type (Street, railroad, Utility)
* Ownership state
* Dice roll
* Rule set
* Ownership counts



\*\*Benefits\*\*

* Pure functions
* Easy to test
* Centralized logic



\### 3. Removed Adapter layer



\*\*Deleted\*\*

* OwnableSpaceTileAdapter



\*\*Changed\*\*

* Rent logic now reads directly from OwnableSpaceData



\*\*Reason\*\*

The adapter introduced an unnecessary amount of complexity and duplicated data access.  The direct access is cleaner and more maintainable. 



\### 4. Ownership Handling 

* OwnershipServiceAdapter still exists
* Provides:

  * Owner lookup
  * Group Ownership counts
  * Railroad counts
  * Utility ownership checks



\*\*Note\*\*

This is being treated as a service not as a adapter, I just choose to not rename the file 



\### 5. RentManager Refactor



\*\*Changes\*\*

* Uses RentCalculator.ComputeRent()
* Dependencies resolved by GetComponent<>
* Removed FindFirstObjectByType
* Added a fallback to StandardRuleSetof no RulesManager is assigned



\*\*Example flow\*\*

* Player Lands on a tile

  * RentManager.TryChargeRent()
  * RentCalculator.ComputeRent()
  * EconomyAdapter transfers money



\### 6. Rules Dependency Update



* Removed the runtime lookup using FindFirstObjectByType
* RulesManager should be assigned in the Inspector



\*\*Fallback\*\*

* If no RulesManager is present, they StandardRulesSet is used which right now is primarily for testing and will be updated/using actual RulesManager during integration. 



\### 7. Test Updates



\#### Unit Tests

* Replaced StandardRentStrategyTests with RentCalculatorTests
* Tests are no being directly called like RentCalculator.ComputeRent()



\#### Integration Tests

* Updated RentManagerIntegrationTests
* Added helper methods

  * AssertRentTransfer
  * AssertNoMoneyTransfer 



\#### Overall Test Improvements

* Removed dependency on strategy pattern
* Simplified the test setup
* Made sure there is consistent ownership service usage 



\### 8. Dependency Handling Fixes

* Fixed Duplicate component issues in tests
* Made sure sharded OwnershipServiceAdapter instance is used
* Added explicit component setup in test base



\## Design Improvements 



\*\*Before\*\*

* RentManager

  * Tile Adapter
  * Strategy (StandardRentStrategy)
  * OwnableSpaceData



\*\*After\*\*

* RentManager

  * RentCalculator (static)
  * OwnableSpaceData



\### Benefits

* Reduced Compliexty 
* Few classes and abstractions
* Easier debugging
* Improved testability
* Clearer Data flow
* Better Separation of concerns



\### Risks

* Since strategy pattern is removed there is less flexibility for future expansions 
* Future changes like different rule variations might mean there needs to be abstraction reintroduced
* Current implement assumes a single rent calculation model, which is okay for now but worth mentioning



\### Improvements that Could Be Made

* Replace the OwnershipServiceAdapter with a centralized ownership system
* Add UI feedback for rent calculation 
* Extend the modifier system for events
* Add logging for rent calculations 



\## Summary



This refactor simplifies the rent system by removing the unnecessary abstraction and consolidates the logic into a static calculator class.  The result is a more maintainable, testable, and understandable system that will be more aligned with the project's overall architectural goals. 

