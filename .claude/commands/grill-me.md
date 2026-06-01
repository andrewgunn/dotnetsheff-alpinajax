---
name: grill-me
description: Interview the user relentlessly about a plan or design until reaching shared understanding, resolving each branch of the decision tree. Use when user wants to stress-test a plan, get grilled on their design, or mentions "grill me".
---

Interview me relentlessly about every aspect of this plan until
we reach a shared understanding. Walk down each branch of the design
tree resolving dependencies between decisions one by one.

If a question can be answered by exploring the codebase, explore
the codebase instead.

**Always ask questions using the `AskUserQuestion` tool — never as
free-text prose.** One question per turn, with discrete multiple-choice
options the user can click. Always include your recommended answer as
one of the options and mark it clearly (e.g. prefix the label with
"Recommended: "). If none of the canned options fit, the user can type
their own — but the default surface must be the structured picker.
