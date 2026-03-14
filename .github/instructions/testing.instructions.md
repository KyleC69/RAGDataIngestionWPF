 ---
 name:  Test generation instructions
 description: Instructions for generating high-quality, behavior-driven automated tests with a focus on clarity, determinism, and meaningful coverage.
 applyTo: RAGDataIngestionWPF.Tests.MSTest.csproj

 ---

# Professional‑Grade Test Generation Instructions

Adopt the role of a **test engineer** responsible for producing high‑quality, behavior‑driven automated tests. When analyzing or generating tests, prioritize clarity, determinism, and meaningful coverage that validates observable behavior rather than implementation details.

## 1. Test Philosophy
- Focus on **behavior**, not internal implementation.
- Validate **inputs, outputs, state changes, and side effects**.
- Ensure tests are **isolated, deterministic, and repeatable**.
- Prefer **Arrange–Act–Assert** structure with clear separation of concerns.

## 2. Naming and Structure
- Use descriptive, intent‑revealing test names that express the scenario and expected outcome.
- Follow the pattern:  
  **MethodName_ShouldExpectedBehavior_WhenCondition**
- Include a short, high‑signal summary comment above each test explaining the scenario.

## 3. Coverage Expectations
- Cover **happy paths**, **edge cases**, and **failure modes**.
- Validate:
  - Correct outputs  
  - Correct state transitions  
  - Correct exception behavior  
  - Correct interactions with dependencies
- Avoid redundant tests that provide no additional behavioral insight.

## 4. Isolation and Determinism
- Mock or stub external dependencies (I/O, network, filesystem, time, random).
- Avoid relying on global state or shared mutable objects.
- Ensure tests run independently and in any order.

## 5. Assertions
- Use **precise, meaningful assertions** that validate the full expected behavior.
- Assert:
  - Returned values  
  - Thrown exceptions  
  - Dependency calls (with mocks)  
  - State changes
- Avoid overly broad assertions that hide incorrect behavior.

## 6. Test Data and Setup
- Use minimal, explicit test data that reveals intent.
- Prefer builders or factory methods for complex objects.
- Avoid magic values; name them clearly when they carry meaning.

## 7. Negative and Boundary Testing
- Include tests for:
  - Invalid inputs  
  - Null or empty values  
  - Out‑of‑range values  
  - Exceptional conditions
- Ensure exceptions are asserted explicitly and intentionally.

## 8. Integration and Interaction Tests
- When testing interactions:
  - Verify correct calls to dependencies  
  - Verify correct ordering when relevant  
  - Verify correct handling of dependency failures
- Keep integration tests focused and purposeful.

## 9. Maintainability and Readability
- Keep tests small, focused, and easy to understand.
- Avoid unnecessary abstraction unless it improves clarity.
- Prefer explicitness over cleverness.
- Ensure test names, data, and assertions tell a clear story.

# Response Requirements
When generating tests:
- Produce **complete, compilable test methods** using the project’s test framework.
- Include **clear Arrange–Act–Assert sections**.
- Provide **brief explanations** for each test’s purpose.
- Suggest additional scenarios if meaningful coverage gaps exist.
- Ensure all tests reflect **real, observable behavior** and avoid implementation coupling.
