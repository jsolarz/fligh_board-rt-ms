# Copilot Instructions

## Overview
This project follows the iDesign Method for system design and development. The focus is on clean architecture, modularity, and test-driven development.

## Guidelines for Copilot

**IMPORTANT:** When working on this project, always:

1. **Reference this file first** - Check current development status, completed steps, and what's next
2. **Update this file with each change** - Keep the progress log current with new completions
3. **Update the .plan file** - Maintain the development journal with technical details
4. **Follow validation criteria** - Ensure each step meets its requirements before proceeding
5. **Maintain coding standards** - Follow the established patterns and principles

**Before starting any work:**
- Read the "Current Development Status" section
- Check the "Next Up" section for current objectives
- Review "Technical Debt Tracker" for things to avoid

**After completing any work:**
- Update completion status in this file
- Add new commit reference
- Update performance metrics if applicable
- Note any lessons learned or challenges encountered

### The Prime Directive
**Never design against the requirements.**

#### Directives
1. Avoid functional decomposition.
2. Decompose based on volatility.
3. Provide a composable design.
4. Offer features as aspects of integration, not implementation.
5. Design iteratively, build incrementally.
6. Design the project to build the system.
7. Drive educated decisions with viable options that differ by schedule, cost, and risk.
8. Build the project along its critical path.
9. Be on time throughout the project.

### System Design Guidelines

#### 1. Requirements
a. Capture required behavior, not required functionality.
b. Describe required behavior with use cases.
c. Document all use cases that contain nested conditions with activity diagrams.
d. Eliminate solutions masquerading as requirements.
e. Validate the system design by ensuring it supports all core use cases.

#### 2. Cardinality
a. Avoid more than five Managers in a system without subsystems.
b. Avoid more than a handful of subsystems.
c. Avoid more than three Managers per subsystem.
d. Strive for a golden ratio of Engines to Managers.
e. Allow ResourceAccess components to access more than one Resource if necessary.

#### 3. Attributes
a. Volatility should decrease top-down.
b. Reuse should increase top-down.
c. Do not encapsulate changes to the nature of the business.
d. Managers should be almost expendable.
e. Design should be symmetric.
f. Never use a public communication channels for internal system interactions.

#### 4. Layers
a. Avoid open architecture.
b. Avoid semi-closed/semi-open architecture.
c. Prefer a closed architecture.
  i) Do not call up.
  ii) Do not call sideways (except queued calls between Managers).
  iii) Do not call more than one layer down.
  iv) Resolve attempts at opening the architecture by using queued calls or asynchronous event publishing.
d. Extend the system by implementing subsystems.

#### 5. Interaction rules
a. All components can call Utilities.
b. Managers and Engines can call ResourceAccess.
c. Managers can call Engines.
d. Managers can queue calls to another Manager.

#### 6. Interaction don’ts
a. Clients do not call multiple Managers in the same use case.
b. Managers do not queue calls to more than one Manager in the same
use case.
c. Engines do not receive queued calls.
d. ResourceAccess components do not receive queued calls.
e. Clients do not publish events.
f. Engines do not publish events.
g. ResourceAccess components do not publish events.
h. Resources do not publish events.
i. Engines, ResourceAccess, and Resources do not subscribe to events.

### Updated Copilot Instructions

#### System Design Guidelines
- Added a directive to ensure that all frontend interactions with the backend are routed through a **Queue/Bus Proxy**.
- Emphasized the roles of **Manager**, **Engine**, and **Accessor** in the backend architecture.
- Clarified that Managers should queue calls to other Managers when needed, following the IDesign Method's principles.

### Project Design Guidelines
#### General
a. Do not design a clock.
b. Never design a project without an architecture that encapsulates the volatilities.
c. Capture and verify planning assumptions.
d. Follow the design of project design.
e. Design several options for the project; at a minimum, design normal, compressed, and subcritical solutions.
f. Communicate with management in Optionality.
g. Always go through SDP review before the main work starts.

#### Service Contract Design Guidelines
1. Design reusable service contracts.
2. Comply with service contract design metrics
  a. Avoid contracts with a single operation.
  b. Strive to have 3 to 5 operations per service contract.
  c. Avoid service contracts with more than 12 operations.
  d. Reject service contracts with 20 or more operations.
3. Avoid property-like operations.
4. Limit the number of contracts per service to 1 or 2.
5. Avoid junior hand-offs.
6. Have only the architect or competent senior developers design the contracts.

### Implementation Guide for Flight Board Management System
10. **Adhere to Clean Architecture**:
   - Ensure separation of concerns between Domain, Application, Infrastructure, and API layers.
   - Avoid mixing business logic with infrastructure or API code.

11. **Follow TDD Principles**:
   - Write unit tests before implementing features.
   - Use xUnit/NUnit with Moq for backend tests.

12. **Use Modern Frontend Practices**:
   - Use TanStack Query for server state management.
   - Use Redux Toolkit for UI state management.
   - Ensure the UI is responsive and user-friendly.

13. **Real-Time Communication**:
   - Use SignalR for live updates.
   - Avoid polling mechanisms.

14. **Validation and Error Handling**:
   - Implement server-side validation for all API endpoints.
   - Return appropriate HTTP error codes and messages.

16. **Documentation and Code Quality**:
   - Write clean, well-documented code.
   - Use TypeScript and C# typings.

17. **Optional Enhancements**:
   - Add animations for frontend interactions.
   - Implement structured logging in the backend.
   - Provide Docker support for easy deployment.


NEVER create "shadow" files like "some_original_file_new.go" or "_fixed.go" then `rm` or `mv` to replace the original as it simply creates duplicate declarations requiring multiple VSCode restarts. Use Copilot tools to edit files. When deleting, renaming, or fully rewriting files, always use Copilot's file management tools (such as apply_patch, insert_edit_into_file, or create_file) instead of terminal or OS commands. if moving files and a git repository exists use git move to preserve history
