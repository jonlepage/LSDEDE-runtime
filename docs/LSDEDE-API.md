LSDE Dialog Engine — Full API Reference (plain text, auto-generated)
============================================================
Concatenates all TypeDoc-generated API documentation for LLM consumption.
Source: lsde-ts/docs/api-ref/**/*.md
============================================================

# LSDE Dialog Engine

## Classes

- [DialogueEngine](classes/DialogueEngine.md)
- [LsdeUtils](classes/LsdeUtils.md)

## Interfaces

- [ActionCall](interfaces/ActionCall.md)
- [ActionContext](interfaces/ActionContext.md)
- [BaseBlockContext](interfaces/BaseBlockContext.md)
- [BeforeBlockArgs](interfaces/BeforeBlockArgs.md)
- [BeforeBlockContext](interfaces/BeforeBlockContext.md)
- [Block](interfaces/Block.md)
- [BlockHandlerArgs](interfaces/BlockHandlerArgs.md)
- [Blueprints](interfaces/Blueprints.md)
- [Card](interfaces/Card.md)
- [CheckOptions](interfaces/CheckOptions.md)
- [ChoiceContext](interfaces/ChoiceContext.md)
- [ConditionCase](interfaces/ConditionCase.md)
- [ConditionContext](interfaces/ConditionContext.md)
- [ConditionTest](interfaces/ConditionTest.md)
- [DiagnosticEntry](interfaces/DiagnosticEntry.md)
- [DiagnosticReport](interfaces/DiagnosticReport.md)
- [DiagnosticStats](interfaces/DiagnosticStats.md)
- [DialogContext](interfaces/DialogContext.md)
- [DictionaryDefinition](interfaces/DictionaryDefinition.md)
- [FunctionDefinition](interfaces/FunctionDefinition.md)
- [FunctionParameter](interfaces/FunctionParameter.md)
- [Generator](interfaces/Generator.md)
- [IDialogueEngine](interfaces/IDialogueEngine.md)
- [InitOptions](interfaces/InitOptions.md)
- [InvalidateBlockArgs](interfaces/InvalidateBlockArgs.md)
- [Link](interfaces/Link.md)
- [NativeProperties](interfaces/NativeProperties.md)
- [Option](interfaces/Option.md)
- [PortResolutionInput](interfaces/PortResolutionInput.md)
- [PortResolutionResult](interfaces/PortResolutionResult.md)
- [RouterContext](interfaces/RouterContext.md)
- [RuntimeChoiceItem](interfaces/RuntimeChoiceItem.md)
- [RuntimeConditionCase](interfaces/RuntimeConditionCase.md)
- [Scene](interfaces/Scene.md)
- [SceneContext](interfaces/SceneContext.md)
- [SceneHandle](interfaces/SceneHandle.md)
- [SceneLifecycleArgs](interfaces/SceneLifecycleArgs.md)
- [TrackInfo](interfaces/TrackInfo.md)
- [ValidateNextBlockArgs](interfaces/ValidateNextBlockArgs.md)
- [ValidateNextBlockContext](interfaces/ValidateNextBlockContext.md)
- [ValidationResult](interfaces/ValidationResult.md)

## Type Aliases

- [ActionBlock](type-aliases/ActionBlock.md)
- [ActionHandler](type-aliases/ActionHandler.md)
- [BeforeBlockHandler](type-aliases/BeforeBlockHandler.md)
- [BlockHandler](type-aliases/BlockHandler.md)
- [BlockOfType](type-aliases/BlockOfType.md)
- [BlockType](type-aliases/BlockType.md)
- [BlueprintBlock](type-aliases/BlueprintBlock.md)
- [BlueprintConnection](type-aliases/BlueprintConnection.md)
- [BlueprintExport](type-aliases/BlueprintExport.md)
- [BlueprintScene](type-aliases/BlueprintScene.md)
- [CardRole](type-aliases/CardRole.md)
- [ChoiceBlock](type-aliases/ChoiceBlock.md)
- [ChoiceHandler](type-aliases/ChoiceHandler.md)
- [CleanupFn](type-aliases/CleanupFn.md)
- [ConditionBlock](type-aliases/ConditionBlock.md)
- [ConditionEvaluator](type-aliases/ConditionEvaluator.md)
- [ConditionHandler](type-aliases/ConditionHandler.md)
- [ConditionJoin](type-aliases/ConditionJoin.md)
- [ConditionOperator](type-aliases/ConditionOperator.md)
- [DialogBlock](type-aliases/DialogBlock.md)
- [DialogHandler](type-aliases/DialogHandler.md)
- [InvalidateBlockHandler](type-aliases/InvalidateBlockHandler.md)
- [LiteralValueType](type-aliases/LiteralValueType.md)
- [LocaleTable](type-aliases/LocaleTable.md)
- [NoteBlock](type-aliases/NoteBlock.md)
- [PropertyBag](type-aliases/PropertyBag.md)
- [PropertyValue](type-aliases/PropertyValue.md)
- [RouterBlock](type-aliases/RouterBlock.md)
- [SceneLifecycleHandler](type-aliases/SceneLifecycleHandler.md)
- [TextByLocale](type-aliases/TextByLocale.md)
- [ValidateNextBlockHandler](type-aliases/ValidateNextBlockHandler.md)
- [ValueType](type-aliases/ValueType.md)

## Variables

- [BlockType](variables/BlockType.md)
- [CardRole](variables/CardRole.md)
- [ConditionJoin](variables/ConditionJoin.md)
- [ConditionOperator](variables/ConditionOperator.md)
- [LiteralValueType](variables/LiteralValueType.md)
- [NATIVE\_PROPERTY\_IDS](variables/NATIVE_PROPERTY_IDS.md)
- [Ports](variables/Ports.md)
- [ValueType](variables/ValueType.md)

================================================================================

[LSDE Dialog Engine](../index.md) / DialogueEngine

# Class: DialogueEngine

Defined in: [engine.ts:26](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L26)

LSDE Dialog Engine — callback-driven graph dispatcher.

## Implements

- [`IDialogueEngine`](../interfaces/IDialogueEngine.md)

## Constructors

### Constructor

> **new DialogueEngine**(): `DialogueEngine`

#### Returns

`DialogueEngine`

## Methods

### getActiveScenes()

> **getActiveScenes**(): [`SceneHandle`](../interfaces/SceneHandle.md)[]

Defined in: [engine.ts:195](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L195)

Get all currently active scene handles.

#### Returns

[`SceneHandle`](../interfaces/SceneHandle.md)[]

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`getActiveScenes`](../interfaces/IDialogueEngine.md#getactivescenes)

***

### getCurrentBlocks()

> **getCurrentBlocks**(): [`Block`](../interfaces/Block.md)[]

Defined in: [engine.ts:199](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L199)

Get the current block of every active scene.

#### Returns

[`Block`](../interfaces/Block.md)[]

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`getCurrentBlocks`](../interfaces/IDialogueEngine.md#getcurrentblocks)

***

### getSceneConnections()

> **getSceneConnections**(`sceneRef`): [`BlueprintConnection`](../type-aliases/BlueprintConnection.md)[]

Defined in: [engine.ts:215](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L215)

Every wire INSIDE a scene, flattened so each carries the block it leaves.

Graph inspection, for a debug view that wants to see the wiring without playing it. It has
never had anything to do with going from one scene to another: a wire has never crossed a
scene in any version of the format, and chaining two scenes is the game's own business.

#### Parameters

##### sceneRef

`string`

#### Returns

[`BlueprintConnection`](../type-aliases/BlueprintConnection.md)[]

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`getSceneConnections`](../interfaces/IDialogueEngine.md#getsceneconnections)

***

### init()

> **init**(`options`): [`DiagnosticReport`](../interfaces/DiagnosticReport.md)

Defined in: [engine.ts:68](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L68)

Load a payload and report what is wrong with it.

Takes one export, or the several files of a per-scene one — each of those carries the whole
header, so they are folded into a single payload after checking they come from one export.

The engine is initialized only when there are no errors: a payload it cannot read leaves it
unusable rather than half-loaded.

#### Parameters

##### options

[`InitOptions`](../interfaces/InitOptions.md)

#### Returns

[`DiagnosticReport`](../interfaces/DiagnosticReport.md)

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`init`](../interfaces/IDialogueEngine.md#init)

***

### isRunning()

> **isRunning**(): `boolean`

Defined in: [engine.ts:191](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L191)

True if at least one scene is active.

#### Returns

`boolean`

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`isRunning`](../interfaces/IDialogueEngine.md#isrunning)

***

### onAction()

> **onAction**(`handler`): `void`

Defined in: [engine.ts:127](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L127)

Register a global handler for ACTION blocks. The developer MUST handle execution in this handler.

#### Parameters

##### handler

[`ActionHandler`](../type-aliases/ActionHandler.md)

#### Returns

`void`

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`onAction`](../interfaces/IDialogueEngine.md#onaction)

***

### onBeforeBlock()

> **onBeforeBlock**(`handler`): `void`

Defined in: [engine.ts:111](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L111)

Register a handler called before every block. Must call resolve() to continue.

#### Parameters

##### handler

[`BeforeBlockHandler`](../type-aliases/BeforeBlockHandler.md)

#### Returns

`void`

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`onBeforeBlock`](../interfaces/IDialogueEngine.md#onbeforeblock)

***

### onChoice()

> **onChoice**(`handler`): `void`

Defined in: [engine.ts:119](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L119)

Register a global handler for CHOICE blocks. All choices are provided, tagged with `visible` when `onResolveCondition()` is configured.

#### Parameters

##### handler

[`ChoiceHandler`](../type-aliases/ChoiceHandler.md)

#### Returns

`void`

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`onChoice`](../interfaces/IDialogueEngine.md#onchoice)

***

### onCondition()

> **onCondition**(`handler`): `void`

Defined in: [engine.ts:123](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L123)

Register a global handler for CONDITION blocks. The developer MUST handle evaluation in this handler.

#### Parameters

##### handler

[`ConditionHandler`](../type-aliases/ConditionHandler.md)

#### Returns

`void`

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`onCondition`](../interfaces/IDialogueEngine.md#oncondition)

***

### onDialog()

> **onDialog**(`handler`): `void`

Defined in: [engine.ts:115](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L115)

Register a global handler for DIALOG blocks. May return a cleanup function.

#### Parameters

##### handler

[`DialogHandler`](../type-aliases/DialogHandler.md)

#### Returns

`void`

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`onDialog`](../interfaces/IDialogueEngine.md#ondialog)

***

### onInvalidateBlock()

> **onInvalidateBlock**(`handler`): `void`

Defined in: [engine.ts:107](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L107)

Register a handler called when a block fails validation.

#### Parameters

##### handler

[`InvalidateBlockHandler`](../type-aliases/InvalidateBlockHandler.md)

#### Returns

`void`

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`onInvalidateBlock`](../interfaces/IDialogueEngine.md#oninvalidateblock)

***

### onResolveCharacter()

> **onResolveCharacter**(`fn`): `void`

Defined in: [engine.ts:95](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L95)

Register a global character resolver. Called for every block with `metadata.characters`.

#### Parameters

##### fn

(`actors`) => [`Card`](../interfaces/Card.md) \| `undefined`

#### Returns

`void`

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`onResolveCharacter`](../interfaces/IDialogueEngine.md#onresolvecharacter)

***

### onResolveCondition()

> **onResolveCondition**(`evaluator`): `void`

Defined in: [engine.ts:99](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L99)

Install a unified condition evaluator for both choice visibility and condition block pre-evaluation.
The engine handles `choice:` conditions internally via choice history — this callback
evaluates game-state conditions only.

When installed:
- Choice blocks: each choice is tagged with `visible: true | false` based on its `visibilityConditions`.
- Condition blocks: each group is pre-evaluated and the result is available in `context.groups[i].result`
  and `context.evaluation`.

#### Parameters

##### evaluator

(`test`) => `boolean`

#### Returns

`void`

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`onResolveCondition`](../interfaces/IDialogueEngine.md#onresolvecondition)

***

### onSceneEnter()

> **onSceneEnter**(`handler`): `void`

Defined in: [engine.ts:131](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L131)

Register a handler called when any scene starts.

#### Parameters

##### handler

[`SceneLifecycleHandler`](../type-aliases/SceneLifecycleHandler.md)

#### Returns

`void`

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`onSceneEnter`](../interfaces/IDialogueEngine.md#onsceneenter)

***

### onSceneExit()

> **onSceneExit**(`handler`): `void`

Defined in: [engine.ts:135](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L135)

Register a handler called when any scene ends (natural or cancelled).

#### Parameters

##### handler

[`SceneLifecycleHandler`](../type-aliases/SceneLifecycleHandler.md)

#### Returns

`void`

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`onSceneExit`](../interfaces/IDialogueEngine.md#onsceneexit)

***

### onValidateNextBlock()

> **onValidateNextBlock**(`handler`): `void`

Defined in: [engine.ts:103](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L103)

Register a handler called before each block to validate it.

#### Parameters

##### handler

[`ValidateNextBlockHandler`](../type-aliases/ValidateNextBlockHandler.md)

#### Returns

`void`

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`onValidateNextBlock`](../interfaces/IDialogueEngine.md#onvalidatenextblock)

***

### scene()

> **scene**(`sceneRef`): [`SceneHandle`](../interfaces/SceneHandle.md)

Defined in: [engine.ts:147](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L147)

Open a scene by its path (`reactor_breach`) or by its stable id (`sc_u0vqg2g8`).

Take the id wherever the reference is stored OUTSIDE the payload — a Unity asset, a save
file, a database row. The path is what a writer reads and what builds the i18n keys, but it
changes the day someone renames the scene, and a serialized path then stops resolving with
no compiler to catch it. The id survives a rename; show the path as its label.

#### Parameters

##### sceneRef

`string`

#### Returns

[`SceneHandle`](../interfaces/SceneHandle.md)

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`scene`](../interfaces/IDialogueEngine.md#scene)

***

### setLocale()

> **setLocale**(`locale`): `void`

Defined in: [engine.ts:82](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L82)

Set the active locale for text resolution.

#### Parameters

##### locale

`string`

#### Returns

`void`

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`setLocale`](../interfaces/IDialogueEngine.md#setlocale)

***

### stop()

> **stop**(): `void`

Defined in: [engine.ts:180](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/engine.ts#L180)

Cancel every running scene.

Every one of them, even if a cleanup throws on the way. A scene left running after `stop()`
is a dialogue the game can no longer see or reach, and one handler's failure must not do
that to the scenes after it — the same rule a scene already applies to its own tracks. The
first fault surfaces once there is nothing left to close.

#### Returns

`void`

#### Implementation of

[`IDialogueEngine`](../interfaces/IDialogueEngine.md).[`stop`](../interfaces/IDialogueEngine.md#stop)

================================================================================

[LSDE Dialog Engine](../index.md) / LsdeUtils

# Class: LsdeUtils

Defined in: [lsde-utils.ts:33](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L33)

Public utility class exposing common helpers for game developers integrating the LSDE engine.

## Constructors

### Constructor

> **new LsdeUtils**(): `LsdeUtils`

#### Returns

`LsdeUtils`

## Properties

### evaluateConditionCases

> `static` **evaluateConditionCases**: (`cases`, `portPerCase`, `evaluator`) => `string`

Defined in: [lsde-utils.ts:184](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L184)

The exit port of a condition block: `out`/`default` in if mode, `K1`… with `portPerCase`.
Replaces the v1 `evaluateConditionGroups`, which returned an index and had a third,
dispatcher mode that no longer exists.

Pick the exit port of a condition block. There are two modes and only two.

| `portPerCase` | rule | exit |
|---|---|---|
| absent | every case must hold | `out` if they all do, `default` otherwise |
| `true` | the first case that holds, in order | its own port (`K1`…), `default` if none |

A case with no `when` is always true — and makes every case below it unreachable in
`portPerCase` mode. That is the writer's drawing, not an error to report.

A block with no cases at all leaves by `out`: nothing was asked, so nothing failed.

#### Parameters

##### cases

[`ConditionCase`](../interfaces/ConditionCase.md)[] \| `undefined`

##### portPerCase

`boolean`

##### evaluator

[`ConditionEvaluator`](../type-aliases/ConditionEvaluator.md)

#### Returns

`string`

***

### evaluateConditionChain

> `static` **evaluateConditionChain**: (`tests`, `evaluator`) => `boolean`

Defined in: [lsde-utils.ts:177](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L177)

Evaluate a chain of tests left to right, with NO operator precedence. Absent or empty
means true — which is how "always" is written in v2.

Evaluate a chain of tests left to right, **with no operator precedence**.

`a AND b OR c` reads as `(a AND b) OR c`, never as `a AND (b OR c)`. That is deliberate: the
editor draws a flat list, so the engine evaluates a flat list. A writer who needs grouping uses
two condition blocks in a row, which is also what the reader of the graph sees.

`join` links a test to the one ABOVE it and is absent on the first. Missing means AND.

**Every test is evaluated, even once the answer is settled.** No short-circuit: the game's
evaluator is also where a project logs, counts or displays what was asked, and skipping calls
would make that log depend on the order the writer happened to use.

No tests at all = true. That is how "always" is written in v2 — by the ABSENCE of `when`,
never by an empty list.

#### Parameters

##### tests

[`ConditionTest`](../interfaces/ConditionTest.md)[] \| `undefined`

##### evaluator

[`ConditionEvaluator`](../type-aliases/ConditionEvaluator.md)

#### Returns

`boolean`

***

### evaluateEachCase

> `static` **evaluateEachCase**: (`cases`, `evaluator`) => `boolean`[]

Defined in: [lsde-utils.ts:187](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L187)

Each case on its own, in order — to show what matched without changing where the flow goes.

Evaluate every case on its own, without picking a port.

Handed to a game that wants to show what matched without changing where the flow goes. The
engine fills `context.cases[i].result` with the same rule, then reads the exit port off those
results, by the same rule [evaluateConditionCases](#evaluateconditioncases) applies — never by calling this and
deciding for itself.

#### Parameters

##### cases

[`ConditionCase`](../interfaces/ConditionCase.md)[] \| `undefined`

##### evaluator

[`ConditionEvaluator`](../type-aliases/ConditionEvaluator.md)

#### Returns

`boolean`[]

***

### isActionBlock

> `static` **isActionBlock**: (`block`) => `block is ActionBlock`

Defined in: [lsde-utils.ts:49](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L49)

Returns `true` if the block is an action.

#### Parameters

##### block

[`Block`](../interfaces/Block.md)

#### Returns

`block is ActionBlock`

***

### isChoiceBlock

> `static` **isChoiceBlock**: (`block`) => `block is ChoiceBlock`

Defined in: [lsde-utils.ts:43](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L43)

Returns `true` if the block is a choice.

#### Parameters

##### block

[`Block`](../interfaces/Block.md)

#### Returns

`block is ChoiceBlock`

***

### isChoiceCondition

> `static` **isChoiceCondition**: (`test`) => `boolean` = `isChoiceTest`

Defined in: [lsde-utils.ts:166](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L166)

Does this test read a past answer of the player rather than game state?

`choice` is a reserved dictionary id that no project dictionary may take: `entry` is a CHOICE
block id of this scene, `value` an option id of that block. The engine answers these from
its own history, so a game never has to remember what it already told the engine.

Is this test about what the player already answered, rather than about game state?

`choice` is a reserved dictionary id — no project dictionary may take it. `entry` is a CHOICE
block id of this scene and `value` an option id of that block. The engine answers these from
the history it kept during the scene, so they never reach the game's evaluator: a game does not
have to remember what it already told the engine.

#### Parameters

##### test

[`ConditionTest`](../interfaces/ConditionTest.md)

#### Returns

`boolean`

***

### isConditionBlock

> `static` **isConditionBlock**: (`block`) => `block is ConditionBlock`

Defined in: [lsde-utils.ts:45](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L45)

Returns `true` if the block is a condition.

#### Parameters

##### block

[`Block`](../interfaces/Block.md)

#### Returns

`block is ConditionBlock`

***

### isDialogBlock

> `static` **isDialogBlock**: (`block`) => `block is DialogBlock`

Defined in: [lsde-utils.ts:41](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L41)

Returns `true` if the block is a dialog.

#### Parameters

##### block

[`Block`](../interfaces/Block.md)

#### Returns

`block is DialogBlock`

***

### isNoteBlock

> `static` **isNoteBlock**: (`block`) => `block is NoteBlock`

Defined in: [lsde-utils.ts:51](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L51)

Returns `true` if the block is a note.

#### Parameters

##### block

[`Block`](../interfaces/Block.md)

#### Returns

`block is NoteBlock`

***

### isRouterBlock

> `static` **isRouterBlock**: (`block`) => `block is RouterBlock`

Defined in: [lsde-utils.ts:47](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L47)

Returns `true` if the block is a router.

#### Parameters

##### block

[`Block`](../interfaces/Block.md)

#### Returns

`block is RouterBlock`

***

### locale

> `static` **locale**: `string` \| `null` = `null`

Defined in: [lsde-utils.ts:36](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L36)

Current locale set by `engine.setLocale()`. Used as the default by the text helpers.

***

### pickRouterPorts

> `static` **pickRouterPorts**: (`cases`, `results`) => `string`[]

Defined in: [lsde-utils.ts:194](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L194)

The exits of a ROUTER, from case results already computed: the port of every true case, then
`then` when they all held or `catch` when one did not — always last. The router's reading of
the same `cases` a condition carries.

A ROUTER's exits: the port of every true case, then `then` or `catch`.

The opposite reading of the same `cases` a condition carries. A condition asks *which one* and
leaves by a single port; a router asks *which ones*, launches each of them, and continues
besides — by `then` when every case held, by `catch` when any did not.

Three rules this encodes, all of them from the format's own contract:

- **No break.** Every case is counted, so a false one in the middle does not hide the true ones
  after it. That is the whole difference with a condition.
- **The tally is over CASES, not over ports.** A port carrying several wires launches several
  tracks and still counts as one case — and two cases wired to the same block dispatch it twice.
- **No cases at all → `then`**, the way `Promise.all([])` resolves.

The continuation is LAST in the list on purpose: the traversal keeps the first non-async target
as the main flow, so `then`/`catch` stays the main flow as long as the case routes are async.

`catch` cancels nothing. The tracks of the true cases are already running by the time the tally
is read — exactly like a `Promise.all` that rejects while its promises carry on.

#### Parameters

##### cases

[`ConditionCase`](../interfaces/ConditionCase.md)[] \| `undefined`

##### results

`boolean`[]

#### Returns

`string`[]

***

### tagOptionVisibility

> `static` **tagOptionVisibility**: (`options`, `evaluator`) => [`RuntimeChoiceItem`](../interfaces/RuntimeChoiceItem.md)[]

Defined in: [lsde-utils.ts:201](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L201)

Tag every option of a choice with whether its `when` holds, returning them ALL.
Replaces the v1 `filterVisibleChoices`, which shortened the list and took away the ability
to show a locked answer.

Tag every option of a choice with whether its `when` holds.

The engine hands over **all** the options, tagged — never a shortened list. A game that wants
only the offered ones writes `options.filter( o => o.visible !== false )`; a game that wants to
grey out the others, or show "[locked]", still has them. Filtering here would take that away.

`visible` is left `undefined` when no evaluator is installed: unknown, not hidden.

#### Parameters

##### options

[`Option`](../interfaces/Option.md)[] \| `undefined`

##### evaluator

[`ConditionEvaluator`](../type-aliases/ConditionEvaluator.md) \| `undefined`

#### Returns

[`RuntimeChoiceItem`](../interfaces/RuntimeChoiceItem.md)[]

## Methods

### getBlockLabel()

> `static` **getBlockLabel**(`block`): `string`

Defined in: [lsde-utils.ts:62](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L62)

How to name a block on screen or in a log.

There is no mandatory block name in v2, and none is needed: `DIALOG-007` already reads
better than the uuid it replaced. A designer note says far more than a three-word label
would, so it comes next; a `label` wins when an export carries one.

#### Parameters

##### block

[`Block`](../interfaces/Block.md)

#### Returns

`string`

***

### getChoiceConditionBlockId()

> `static` **getChoiceConditionBlockId**(`test`): `string` \| `undefined`

Defined in: [lsde-utils.ts:169](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L169)

The CHOICE block a `choice:` test reads, or `undefined` for any other test.

#### Parameters

##### test

[`ConditionTest`](../interfaces/ConditionTest.md)

#### Returns

`string` \| `undefined`

***

### getCustomProperties()

> `static` **getCustomProperties**(`block`): [`PropertyBag`](../type-aliases/PropertyBag.md)

Defined in: [lsde-utils.ts:145](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L145)

The properties the DESIGNER declared, with the natives taken out — everything the game is
free to give its own meaning to.

#### Parameters

##### block

[`Block`](../interfaces/Block.md)

#### Returns

[`PropertyBag`](../type-aliases/PropertyBag.md)

***

### getLocalizedText()

> `static` **getLocalizedText**(`text`, `locale?`): `string` \| `undefined`

Defined in: [lsde-utils.ts:74](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L74)

Pick a locale out of an inline `text` map — `block.text`, or an option's.

Only works when texts were exported INSIDE the payload. With the separate mode, the blocks
carry no `text` at all and [getTextFromTable](#gettextfromtable) is the one to use.

#### Parameters

##### text

[`TextByLocale`](../type-aliases/TextByLocale.md) \| `undefined`

##### locale?

`string`

#### Returns

`string` \| `undefined`

#### Throws

when no locale is set, by parameter or by `engine.setLocale()`.

***

### getNativeProperties()

> `static` **getNativeProperties**(`block`): [`NativeProperties`](../interfaces/NativeProperties.md)

Defined in: [lsde-utils.ts:130](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L130)

The properties the ENGINE acts on, pulled out of a block's `props`.

v2 puts natives and the designer's own properties in one bag, keyed by bare id, and ids
cannot collide — LSDE refuses a project property that takes a native name. So this is a
lookup against [NATIVE\_PROPERTY\_IDS](../variables/NATIVE_PROPERTY_IDS.md), not a guess.

**`delay` and `timeout` are MILLISECONDS.** They were seconds in v1 and nothing reports the
change at runtime: a migrated project turns a 3-second pause into 3 ms.

#### Parameters

##### block

[`Block`](../interfaces/Block.md)

#### Returns

[`NativeProperties`](../interfaces/NativeProperties.md)

***

### getTextFromTable()

> `static` **getTextFromTable**(`table`, `scene`, `blockId`, `optionId?`): `string` \| `undefined`

Defined in: [lsde-utils.ts:91](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L91)

Read a line out of a loaded `localization/<locale>/__blueprints__.json`, for the separate
text mode.

The game loads the file — the engine does no IO, ever. Pass the block's scene and id, plus
an option id for one answer of a choice.

```ts
const fr = JSON.parse( await readFile( 'localization/fr/__blueprints__.json', 'utf-8' ) );
LsdeUtils.getTextFromTable( fr, 'reactor_breach', 'DIALOG-001' );
LsdeUtils.getTextFromTable( fr, 'reactor_breach', 'CHOICE-001', 'C1' );
```

#### Parameters

##### table

[`LocaleTable`](../type-aliases/LocaleTable.md) \| `undefined`

##### scene

`string`

##### blockId

`string`

##### optionId?

`string`

#### Returns

`string` \| `undefined`

***

### getTextKey()

> `static` **getTextKey**(`block`, `optionId?`): `string`

Defined in: [lsde-utils.ts:114](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L114)

The i18n key of a block, or of one option of a choice.

The key is already in the payload (`block.key`), so this only builds the option variant —
useful for a voice file, whose name is derived from the key.

#### Parameters

##### block

[`Block`](../interfaces/Block.md)

##### optionId?

`string`

#### Returns

`string`

================================================================================

[LSDE Dialog Engine](../index.md) / ActionCall

# Interface: ActionCall

Defined in: [blueprint-types.ts:148](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L148)

What an action block asks the engine to run.

## Properties

### args

> **args**: [`PropertyBag`](../type-aliases/PropertyBag.md)

Defined in: [blueprint-types.ts:152](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L152)

The arguments BY NAME, as declared in FunctionDefinition.params.

***

### fn

> **fn**: `string`

Defined in: [blueprint-types.ts:150](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L150)

The function id. May be empty when the designer has not picked one yet.

================================================================================

[LSDE Dialog Engine](../index.md) / ActionContext

# Interface: ActionContext

Defined in: [types.ts:381](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L381)

What an ACTION handler gets.

## Extends

- [`BaseBlockContext`](BaseBlockContext.md)

## Properties

### actors

> **actors**: [`Card`](Card.md)[]

Defined in: [types.ts:309](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L309)

Every card the block cites, resolved through the export's `cards` table, in file order.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`actors`](BaseBlockContext.md#actors)

***

### calls

> **calls**: [`ActionCall`](ActionCall.md)[]

Defined in: [types.ts:383](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L383)

The calls the block asks the game to run, in order, with their arguments BY NAME.

***

### character

> **character**: [`Card`](Card.md) \| `undefined`

Defined in: [types.ts:307](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L307)

The actor `onResolveCharacter()` picked for this block, or `undefined`.

A block lists a CAST in `actors` — card ids, in an order LSDE deliberately refuses to give a
meaning to. Whether the first one speaks, whether they all do, whether the rest are simply
present is the game's call, so the engine hands the whole list to `onResolveCharacter()` and
keeps whatever comes back. It does not elect a first one, the way v1 did.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`character`](BaseBlockContext.md#character)

***

### emotion

> **emotion**: [`Card`](Card.md) \| `undefined`

Defined in: [types.ts:317](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L317)

The emotion of the block, resolved through `cards` — the TONE of the line, not of a speaker.

In v1 each character carried its own emotion, which meant writing the same feeling twice for
two actors saying one sentence, and being free to desynchronise them by accident. A block is
one line and one line has one tone; `actors` says who may carry it.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`emotion`](BaseBlockContext.md#emotion)

***

### intensity

> **intensity**: `number` \| `undefined`

Defined in: [types.ts:319](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L319)

How strongly, when the writer set an emotion. Passed through untouched.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`intensity`](BaseBlockContext.md#intensity)

***

### preventGlobalHandler

> **preventGlobalHandler**: () => `void`

Defined in: [types.ts:321](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L321)

Stop the global (Tier 1) handler from running after this scene handler.

#### Returns

`void`

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`preventGlobalHandler`](BaseBlockContext.md#preventglobalhandler)

***

### reject

> **reject**: (`error?`) => `void`

Defined in: [types.ts:393](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L393)

A call failed. The flow leaves by `catch`, or by `then` when no error branch was drawn.

The error is OPTIONAL and the engine does nothing with it: routing only needs to know that
the call failed. Pass one if it reads better next to your own logging — nothing here reads
it, forwards it or logs it.

#### Parameters

##### error?

`unknown`

#### Returns

`void`

***

### resolve

> **resolve**: () => `void`

Defined in: [types.ts:385](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L385)

The calls went through. The flow leaves by `then`.

#### Returns

`void`

================================================================================

[LSDE Dialog Engine](../index.md) / BaseBlockContext

# Interface: BaseBlockContext

Defined in: [types.ts:298](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L298)

What every block handler gets, whatever the block type.

## Extended by

- [`DialogContext`](DialogContext.md)
- [`ChoiceContext`](ChoiceContext.md)
- [`ConditionContext`](ConditionContext.md)
- [`RouterContext`](RouterContext.md)
- [`ActionContext`](ActionContext.md)

## Properties

### actors

> **actors**: [`Card`](Card.md)[]

Defined in: [types.ts:309](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L309)

Every card the block cites, resolved through the export's `cards` table, in file order.

***

### character

> **character**: [`Card`](Card.md) \| `undefined`

Defined in: [types.ts:307](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L307)

The actor `onResolveCharacter()` picked for this block, or `undefined`.

A block lists a CAST in `actors` — card ids, in an order LSDE deliberately refuses to give a
meaning to. Whether the first one speaks, whether they all do, whether the rest are simply
present is the game's call, so the engine hands the whole list to `onResolveCharacter()` and
keeps whatever comes back. It does not elect a first one, the way v1 did.

***

### emotion

> **emotion**: [`Card`](Card.md) \| `undefined`

Defined in: [types.ts:317](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L317)

The emotion of the block, resolved through `cards` — the TONE of the line, not of a speaker.

In v1 each character carried its own emotion, which meant writing the same feeling twice for
two actors saying one sentence, and being free to desynchronise them by accident. A block is
one line and one line has one tone; `actors` says who may carry it.

***

### intensity

> **intensity**: `number` \| `undefined`

Defined in: [types.ts:319](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L319)

How strongly, when the writer set an emotion. Passed through untouched.

***

### preventGlobalHandler

> **preventGlobalHandler**: () => `void`

Defined in: [types.ts:321](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L321)

Stop the global (Tier 1) handler from running after this scene handler.

#### Returns

`void`

================================================================================

[LSDE Dialog Engine](../index.md) / BeforeBlockArgs

# Interface: BeforeBlockArgs

Defined in: [types.ts:541](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L541)

Arguments for the onBeforeBlock handler.

## Properties

### block

> **block**: [`Block`](Block.md)

Defined in: [types.ts:542](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L542)

***

### context

> **context**: [`BeforeBlockContext`](BeforeBlockContext.md)

Defined in: [types.ts:544](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L544)

***

### resolve

> **resolve**: () => `void`

Defined in: [types.ts:545](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L545)

#### Returns

`void`

***

### scene

> **scene**: [`SceneHandle`](SceneHandle.md)

Defined in: [types.ts:543](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L543)

================================================================================

[LSDE Dialog Engine](../index.md) / BeforeBlockContext

# Interface: BeforeBlockContext

Defined in: [types.ts:397](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L397)

What `onBeforeBlock` gets.

## Properties

### nativeProperties

> **nativeProperties**: [`NativeProperties`](NativeProperties.md) \| `undefined`

Defined in: [types.ts:399](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L399)

The engine-facing properties of the block, read out of `props`.

================================================================================

[LSDE Dialog Engine](../index.md) / Block

# Interface: Block

Defined in: [blueprint-types.ts:190](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L190)

A node of the graph. type decides which optional fields are present.

## Properties

### actors?

> `optional` **actors?**: `string`[]

Defined in: [blueprint-types.ts:202](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L202)

Card ids of who speaks. Resolve them through Blueprints.cards.

***

### body?

> `optional` **body?**: `string`

Defined in: [blueprint-types.ts:210](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L210)

The body of a note block: never translated, only when notes are exported.

***

### calls?

> `optional` **calls?**: [`ActionCall`](ActionCall.md)[]

Defined in: [blueprint-types.ts:216](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L216)

Action blocks: what to run, in order.

***

### cases?

> `optional` **cases?**: [`ConditionCase`](ConditionCase.md)[]

Defined in: [blueprint-types.ts:218](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L218)

Condition AND router blocks: the cases, in evaluation order.

***

### emotion?

> `optional` **emotion?**: `string`

Defined in: [blueprint-types.ts:204](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L204)

Card id of the emotion, when one is set.

***

### id

> **id**: `string`

Defined in: [blueprint-types.ts:192](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L192)

Identity RELATIVE to its scene (DIALOG-002): what links and Scene.start reference.

***

### intensity?

> `optional` **intensity?**: `number`

Defined in: [blueprint-types.ts:206](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L206)

Only with emotion.

***

### key

> **key**: `string`

Defined in: [blueprint-types.ts:194](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L194)

The full i18n key, as localization files carry it.

***

### label?

> `optional` **label?**: `string`

Defined in: [blueprint-types.ts:196](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L196)

The readable name (its summary), when written.

***

### next?

> `optional` **next?**: [`Link`](Link.md)[]

Defined in: [blueprint-types.ts:222](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L222)

Outgoing wires. Absent when nothing leaves the block.

***

### note?

> `optional` **note?**: `string`

Defined in: [blueprint-types.ts:212](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L212)

The team note on the block, only when notes are exported.

***

### options?

> `optional` **options?**: [`Option`](Option.md)[]

Defined in: [blueprint-types.ts:220](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L220)

Choice blocks: the answers, in display order.

***

### parentLabels?

> `optional` **parentLabels?**: `string`[]

Defined in: [blueprint-types.ts:198](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L198)

Readable names above the block, root first. Empty ones are skipped.

***

### props?

> `optional` **props?**: [`PropertyBag`](../type-aliases/PropertyBag.md)

Defined in: [blueprint-types.ts:214](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L214)

Properties SET on the block, native and project-declared alike, by id. Ids never collide: a project property cannot take a native name. One native holds a LIST rather than a scalar: waitForBlocks, an array of block ids OF THIS SCENE the block waits for before it advances - the counterpart of isAsync.

***

### text?

> `optional` **text?**: [`TextByLocale`](../type-aliases/TextByLocale.md)

Defined in: [blueprint-types.ts:208](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L208)

The line by locale, dialogs only, when texts are exported.

***

### type

> **type**: [`BlockType`](../type-aliases/BlockType.md)

Defined in: [blueprint-types.ts:200](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L200)

What the block is.

================================================================================

[LSDE Dialog Engine](../index.md) / BlockHandlerArgs

# Interface: BlockHandlerArgs\<B, C\>

Defined in: [types.ts:430](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L430)

Arguments passed to any block handler.

## Remarks

Every block handler receives this common structure. The generic `B` parameter provides
the block type ([DialogBlock](../type-aliases/DialogBlock.md), [ChoiceBlock](../type-aliases/ChoiceBlock.md), etc.) and `C` provides
the type-specific context ([DialogContext](DialogContext.md), [ChoiceContext](ChoiceContext.md), etc.).

The engine uses a **two-tier handler system**:
1. **Tier 2 (scene)**: registered via `handle.onDialog()`, `handle.onChoice()`, etc.
2. **Tier 1 (global)**: registered via `engine.onDialog()`, `engine.onChoice()`, etc.

When a block is dispatched, the scene handler (Tier 2) is called first. The global handler
(Tier 1) is then called **after**, unless `context.preventGlobalHandler()` was invoked.

A block-specific override via `handle.onBlock(blockId, handler)` takes highest priority.

## See

 - [BlockHandler](../type-aliases/BlockHandler.md) for the handler function signature
 - [SceneHandle](SceneHandle.md) for scene-level handler registration
 - [BaseBlockContext.preventGlobalHandler](BaseBlockContext.md#preventglobalhandler) for suppressing Tier 1

## Type Parameters

### B

`B` *extends* [`BlueprintBlock`](../type-aliases/BlueprintBlock.md)

### C

`C` *extends* [`BaseBlockContext`](BaseBlockContext.md)

## Properties

### block

> **block**: `B`

Defined in: [types.ts:434](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L434)

The block being executed, typed to match the handler (e.g. `DialogBlock` for `onDialog`).

***

### context

> **context**: `C`

Defined in: [types.ts:436](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L436)

Type-specific context providing actions for this block (e.g. selectChoice, resolve).

***

### next

> **next**: () => `void`

Defined in: [types.ts:438](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L438)

Advance the flow to the next block. Must be called exactly once to continue traversal.

#### Returns

`void`

***

### scene

> **scene**: [`SceneHandle`](SceneHandle.md)

Defined in: [types.ts:432](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L432)

The scene handle that owns this block. Use it to inspect state, cancel the scene, etc.

================================================================================

[LSDE Dialog Engine](../index.md) / Blueprints

# Interface: Blueprints

Defined in: [blueprint-types.ts:240](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L240)

The whole file. One scene per file or all of them: the only difference between the two split modes.

## Properties

### cards

> **cards**: [`Card`](Card.md)[]

Defined in: [blueprint-types.ts:260](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L260)

The cards that blocks may cite.

***

### dictionaries

> **dictionaries**: [`DictionaryDefinition`](DictionaryDefinition.md)[]

Defined in: [blueprint-types.ts:256](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L256)

The declared vocabulary, whole, never trimmed to the exported scenes.

***

### exportedAt

> **exportedAt**: `string`

Defined in: [blueprint-types.ts:248](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L248)

ISO 8601 instant of the export.

***

### format

> **format**: `"lsde-blueprints"`

Defined in: [blueprint-types.ts:242](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L242)

Always lsde-blueprints.

***

### functions

> **functions**: [`FunctionDefinition`](FunctionDefinition.md)[]

Defined in: [blueprint-types.ts:258](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L258)

The declared engine functions.

***

### generator

> **generator**: [`Generator`](Generator.md)

Defined in: [blueprint-types.ts:246](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L246)

Which software wrote the file.

***

### locales

> **locales**: `string`[]

Defined in: [blueprint-types.ts:252](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L252)

Every locale of the project.

***

### project

> **project**: `string`

Defined in: [blueprint-types.ts:250](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L250)

The project name.

***

### referenceLocale

> **referenceLocale**: `string`

Defined in: [blueprint-types.ts:254](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L254)

The locale that is written first. Empty when the project declares none.

***

### scenes

> **scenes**: [`Scene`](Scene.md)[]

Defined in: [blueprint-types.ts:262](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L262)

The scenes carried by this file.

***

### version

> **version**: `1`

Defined in: [blueprint-types.ts:244](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L244)

The FORMAT version. Bumps only when this contract changes.

================================================================================

[LSDE Dialog Engine](../index.md) / Card

# Interface: Card

Defined in: [blueprint-types.ts:128](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L128)

A card cited by blocks: the other end of Block.actors and Block.emotion.

## Properties

### id

> **id**: `string`

Defined in: [blueprint-types.ts:130](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L130)

The stable editor id (var3) that blocks reference.

***

### name

> **name**: `string`

Defined in: [blueprint-types.ts:132](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L132)

The name the game gives this card, never the editor label.

***

### role

> **role**: [`CardRole`](../type-aliases/CardRole.md)

Defined in: [blueprint-types.ts:134](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L134)

What the card is used for.

================================================================================

[LSDE Dialog Engine](../index.md) / CheckOptions

# Interface: CheckOptions

Defined in: [types.ts:254](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L254)

Options for cross-validating blueprint data against game capabilities. When provided, the engine warns about blueprint references that don't match your game's known capabilities.

## Properties

### cards?

> `optional` **cards?**: `string`[]

Defined in: [types.ts:260](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L260)

Card NAMES your game knows — `card.name`, never the editor id (`var1`).

***

### dictionaries?

> `optional` **dictionaries?**: `Record`\<`string`, `string`[]\>

Defined in: [types.ts:258](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L258)

Dictionary ids and their entry keys, as your game holds them. Anything outside warns.

***

### functions?

> `optional` **functions?**: `string`[]

Defined in: [types.ts:256](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L256)

Function ids your game implements. A blueprint function outside this list warns.

================================================================================

[LSDE Dialog Engine](../index.md) / ChoiceContext

# Interface: ChoiceContext

Defined in: [types.ts:336](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L336)

What a CHOICE handler gets.

## Extends

- [`BaseBlockContext`](BaseBlockContext.md)

## Properties

### actors

> **actors**: [`Card`](Card.md)[]

Defined in: [types.ts:309](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L309)

Every card the block cites, resolved through the export's `cards` table, in file order.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`actors`](BaseBlockContext.md#actors)

***

### character

> **character**: [`Card`](Card.md) \| `undefined`

Defined in: [types.ts:307](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L307)

The actor `onResolveCharacter()` picked for this block, or `undefined`.

A block lists a CAST in `actors` — card ids, in an order LSDE deliberately refuses to give a
meaning to. Whether the first one speaks, whether they all do, whether the rest are simply
present is the game's call, so the engine hands the whole list to `onResolveCharacter()` and
keeps whatever comes back. It does not elect a first one, the way v1 did.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`character`](BaseBlockContext.md#character)

***

### emotion

> **emotion**: [`Card`](Card.md) \| `undefined`

Defined in: [types.ts:317](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L317)

The emotion of the block, resolved through `cards` — the TONE of the line, not of a speaker.

In v1 each character carried its own emotion, which meant writing the same feeling twice for
two actors saying one sentence, and being free to desynchronise them by accident. A block is
one line and one line has one tone; `actors` says who may carry it.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`emotion`](BaseBlockContext.md#emotion)

***

### intensity

> **intensity**: `number` \| `undefined`

Defined in: [types.ts:319](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L319)

How strongly, when the writer set an emotion. Passed through untouched.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`intensity`](BaseBlockContext.md#intensity)

***

### options

> **options**: [`RuntimeChoiceItem`](RuntimeChoiceItem.md)[]

Defined in: [types.ts:344](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L344)

EVERY option of the block, tagged. Not a shortened list.

With `engine.onResolveCondition()` installed, each carries `visible: true | false`; without
one it is `undefined` — unknown, not hidden. Show the offered ones with
`options.filter( o => o.visible !== false )`, or keep the rest to grey them out.

***

### preventGlobalHandler

> **preventGlobalHandler**: () => `void`

Defined in: [types.ts:321](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L321)

Stop the global (Tier 1) handler from running after this scene handler.

#### Returns

`void`

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`preventGlobalHandler`](BaseBlockContext.md#preventglobalhandler)

***

### selectChoice

> **selectChoice**: (`optionId`) => `void`

Defined in: [types.ts:346](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L346)

Pick an option by its id (`C1`). That id is also the port the flow leaves by.

#### Parameters

##### optionId

`string`

#### Returns

`void`

================================================================================

[LSDE Dialog Engine](../index.md) / ConditionCase

# Interface: ConditionCase

Defined in: [blueprint-types.ts:170](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L170)

One case of a condition or a router block: the exit port, and what must hold for it. The data is identical on both; only the engine's reading differs - see BlockType.

## Properties

### port

> **port**: `string`

Defined in: [blueprint-types.ts:172](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L172)

The exit port of this case (K1...), or the block's out when cases share one exit.

***

### when?

> `optional` **when?**: [`ConditionTest`](ConditionTest.md)[]

Defined in: [blueprint-types.ts:174](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L174)

Absent = always true. Such a case makes every following case unreachable.

================================================================================

[LSDE Dialog Engine](../index.md) / ConditionContext

# Interface: ConditionContext

Defined in: [types.ts:350](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L350)

What a CONDITION handler gets.

## Extends

- [`BaseBlockContext`](BaseBlockContext.md)

## Properties

### actors

> **actors**: [`Card`](Card.md)[]

Defined in: [types.ts:309](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L309)

Every card the block cites, resolved through the export's `cards` table, in file order.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`actors`](BaseBlockContext.md#actors)

***

### cases

> **cases**: [`RuntimeConditionCase`](RuntimeConditionCase.md)[]

Defined in: [types.ts:357](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L357)

The block's cases, each with its port and its pre-evaluated `result`.

With `onResolveCondition()` installed the engine has already evaluated them and already
knows where to go — the handler becomes a place to log or to override, and is optional.

***

### character

> **character**: [`Card`](Card.md) \| `undefined`

Defined in: [types.ts:307](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L307)

The actor `onResolveCharacter()` picked for this block, or `undefined`.

A block lists a CAST in `actors` — card ids, in an order LSDE deliberately refuses to give a
meaning to. Whether the first one speaks, whether they all do, whether the rest are simply
present is the game's call, so the engine hands the whole list to `onResolveCharacter()` and
keeps whatever comes back. It does not elect a first one, the way v1 did.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`character`](BaseBlockContext.md#character)

***

### emotion

> **emotion**: [`Card`](Card.md) \| `undefined`

Defined in: [types.ts:317](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L317)

The emotion of the block, resolved through `cards` — the TONE of the line, not of a speaker.

In v1 each character carried its own emotion, which meant writing the same feeling twice for
two actors saying one sentence, and being free to desynchronise them by accident. A block is
one line and one line has one tone; `actors` says who may carry it.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`emotion`](BaseBlockContext.md#emotion)

***

### intensity

> **intensity**: `number` \| `undefined`

Defined in: [types.ts:319](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L319)

How strongly, when the writer set an emotion. Passed through untouched.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`intensity`](BaseBlockContext.md#intensity)

***

### preventGlobalHandler

> **preventGlobalHandler**: () => `void`

Defined in: [types.ts:321](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L321)

Stop the global (Tier 1) handler from running after this scene handler.

#### Returns

`void`

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`preventGlobalHandler`](BaseBlockContext.md#preventglobalhandler)

***

### resolve

> **resolve**: (`port`) => `void`

Defined in: [types.ts:364](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L364)

Override the exit port. Takes a PORT NAME: `out`, `default`, or a case port (`K1`).

v1 took `boolean | number | number[]` — three shapes for one method, the third being the
dispatcher. Both are gone: a condition picks one path.

#### Parameters

##### port

`string`

#### Returns

`void`

================================================================================

[LSDE Dialog Engine](../index.md) / ConditionTest

# Interface: ConditionTest

Defined in: [blueprint-types.ts:156](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L156)

One comparison: a dictionary entry against a value. The reserved dict id "choice" is the exception — it reads the answers the player already gave IN THIS SCENE, which the engine tracks on its own. No project dictionary can be named that.

## Properties

### dict

> **dict**: `string`

Defined in: [blueprint-types.ts:158](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L158)

The dictionary id, see DictionaryDefinition.id. "choice" is reserved: see Ports.Choice.

***

### entry

> **entry**: `string`

Defined in: [blueprint-types.ts:160](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L160)

The entry read in that dictionary. With dict "choice", a CHOICE block id of this scene.

***

### join?

> `optional` **join?**: [`ConditionJoin`](../type-aliases/ConditionJoin.md)

Defined in: [blueprint-types.ts:166](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L166)

Link with the comparison ABOVE. Absent on the first one.

***

### op

> **op**: [`ConditionOperator`](../type-aliases/ConditionOperator.md)

Defined in: [blueprint-types.ts:162](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L162)

The comparison.

***

### value

> **value**: [`PropertyValue`](../type-aliases/PropertyValue.md)

Defined in: [blueprint-types.ts:164](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L164)

The right-hand side; its type follows the dictionary's valueType. With dict "choice", the Option.id the player picked.

================================================================================

[LSDE Dialog Engine](../index.md) / DiagnosticEntry

# Interface: DiagnosticEntry

Defined in: [types.ts:220](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L220)

Single diagnostic entry (error or warning).

## Properties

### blockId?

> `optional` **blockId?**: `string`

Defined in: [types.ts:236](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L236)

Id of the block where the issue was found, if applicable.

***

### code

> **code**: `string`

Defined in: [types.ts:230](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L230)

Machine-readable code, e.g. `BROKEN_LINK` or `UNKNOWN_WAIT_BLOCK`.

The seventeen the engine emits are listed in the Getting Started guide, split into the
eleven that refuse the payload and the six that let it play. It is a `string` and not a
union on purpose: a runtime is allowed to add one — TypeScript and GDScript read the raw
payload and can say `WRONG_NAMING_CONVENTION`, where C# and C++ only ever see a typed
object and report `INVALID_FORMAT` for the same file.

***

### message

> **message**: `string`

Defined in: [types.ts:232](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L232)

Human-readable description of the issue.

***

### sceneId?

> `optional` **sceneId?**: `string`

Defined in: [types.ts:234](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L234)

Id of the scene where the issue was found, if applicable.

================================================================================

[LSDE Dialog Engine](../index.md) / DiagnosticReport

# Interface: DiagnosticReport

Defined in: [types.ts:247](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L247)

Result of `engine.init()` — validation report.

## Properties

### errors

> **errors**: [`DiagnosticEntry`](DiagnosticEntry.md)[]

Defined in: [types.ts:248](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L248)

***

### stats

> **stats**: [`DiagnosticStats`](DiagnosticStats.md)

Defined in: [types.ts:250](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L250)

***

### warnings

> **warnings**: [`DiagnosticEntry`](DiagnosticEntry.md)[]

Defined in: [types.ts:249](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L249)

================================================================================

[LSDE Dialog Engine](../index.md) / DiagnosticStats

# Interface: DiagnosticStats

Defined in: [types.ts:240](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L240)

Aggregate statistics from blueprint validation.

## Properties

### blockCount

> **blockCount**: `number`

Defined in: [types.ts:242](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L242)

***

### connectionCount

> **connectionCount**: `number`

Defined in: [types.ts:243](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L243)

***

### sceneCount

> **sceneCount**: `number`

Defined in: [types.ts:241](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L241)

================================================================================

[LSDE Dialog Engine](../index.md) / DialogContext

# Interface: DialogContext

Defined in: [types.ts:325](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L325)

What a DIALOG handler gets.

## Extends

- [`BaseBlockContext`](BaseBlockContext.md)

## Properties

### actors

> **actors**: [`Card`](Card.md)[]

Defined in: [types.ts:309](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L309)

Every card the block cites, resolved through the export's `cards` table, in file order.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`actors`](BaseBlockContext.md#actors)

***

### character

> **character**: [`Card`](Card.md) \| `undefined`

Defined in: [types.ts:307](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L307)

The actor `onResolveCharacter()` picked for this block, or `undefined`.

A block lists a CAST in `actors` — card ids, in an order LSDE deliberately refuses to give a
meaning to. Whether the first one speaks, whether they all do, whether the rest are simply
present is the game's call, so the engine hands the whole list to `onResolveCharacter()` and
keeps whatever comes back. It does not elect a first one, the way v1 did.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`character`](BaseBlockContext.md#character)

***

### emotion

> **emotion**: [`Card`](Card.md) \| `undefined`

Defined in: [types.ts:317](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L317)

The emotion of the block, resolved through `cards` — the TONE of the line, not of a speaker.

In v1 each character carried its own emotion, which meant writing the same feeling twice for
two actors saying one sentence, and being free to desynchronise them by accident. A block is
one line and one line has one tone; `actors` says who may carry it.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`emotion`](BaseBlockContext.md#emotion)

***

### intensity

> **intensity**: `number` \| `undefined`

Defined in: [types.ts:319](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L319)

How strongly, when the writer set an emotion. Passed through untouched.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`intensity`](BaseBlockContext.md#intensity)

***

### preventGlobalHandler

> **preventGlobalHandler**: () => `void`

Defined in: [types.ts:321](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L321)

Stop the global (Tier 1) handler from running after this scene handler.

#### Returns

`void`

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`preventGlobalHandler`](BaseBlockContext.md#preventglobalhandler)

***

### resolveCharacterPort

> **resolveCharacterPort**: (`cardId`) => `void`

Defined in: [types.ts:332](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L332)

With `portPerCharacter`, name the actor whose port the flow should take.

Takes a CARD ID (`var1`) — the same id `block.actors` lists and the same one the port is
named after. A card the block does not cite, or one with no port drawn, falls back to `out`.

#### Parameters

##### cardId

`string`

#### Returns

`void`

================================================================================

[LSDE Dialog Engine](../index.md) / DictionaryDefinition

# Interface: DictionaryDefinition

Defined in: [blueprint-types.ts:100](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L100)

A dictionary the engine maintains, as declared in the project. Conditions cite it by id.

## Properties

### entries

> **entries**: `string`[]

Defined in: [blueprint-types.ts:106](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L106)

The entry keys, in declaration order.

***

### id

> **id**: `string`

Defined in: [blueprint-types.ts:102](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L102)

The dictionary id, as ConditionTest.dict cites it.

***

### valueType

> **valueType**: [`LiteralValueType`](../type-aliases/LiteralValueType.md)

Defined in: [blueprint-types.ts:104](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L104)

What its entries are compared to.

================================================================================

[LSDE Dialog Engine](../index.md) / FunctionDefinition

# Interface: FunctionDefinition

Defined in: [blueprint-types.ts:120](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L120)

A function the engine implements, as ActionCall.fn names it.

## Properties

### id

> **id**: `string`

Defined in: [blueprint-types.ts:122](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L122)

The function id.

***

### params

> **params**: [`FunctionParameter`](FunctionParameter.md)[]

Defined in: [blueprint-types.ts:124](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L124)

Its parameters, in declaration order.

================================================================================

[LSDE Dialog Engine](../index.md) / FunctionParameter

# Interface: FunctionParameter

Defined in: [blueprint-types.ts:110](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L110)

One parameter of an engine function.

## Properties

### dictionary?

> `optional` **dictionary?**: `string`

Defined in: [blueprint-types.ts:116](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L116)

Only when type is dictionaryKey: where the value is picked.

***

### name

> **name**: `string`

Defined in: [blueprint-types.ts:112](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L112)

The argument name, as ActionCall.args keys it.

***

### type

> **type**: [`ValueType`](../type-aliases/ValueType.md)

Defined in: [blueprint-types.ts:114](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L114)

What the argument holds.

================================================================================

[LSDE Dialog Engine](../index.md) / Generator

# Interface: Generator

Defined in: [blueprint-types.ts:92](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L92)

Which software wrote the file, to trace a delivered payload back to its version.

## Properties

### app

> **app**: `"LSDE"`

Defined in: [blueprint-types.ts:94](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L94)

Always LSDE.

***

### version

> **version**: `string`

Defined in: [blueprint-types.ts:96](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L96)

The software version, e.g. 2.0.3 - not the format version.

================================================================================

[LSDE Dialog Engine](../index.md) / IDialogueEngine

# Interface: IDialogueEngine

Defined in: [types.ts:681](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L681)

Public interface for the dialogue engine facade.

## Remarks

This is the top-level entry point for the LSDEDE runtime. It manages blueprint loading,
global handler registration, and scene creation. Use [SceneHandle](SceneHandle.md) for per-scene control.

## See

[SceneHandle](SceneHandle.md) for per-scene runtime control

## Methods

### getActiveScenes()

> **getActiveScenes**(): [`SceneHandle`](SceneHandle.md)[]

Defined in: [types.ts:756](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L756)

Get all currently active scene handles.

#### Returns

[`SceneHandle`](SceneHandle.md)[]

***

### getCurrentBlocks()

> **getCurrentBlocks**(): [`Block`](Block.md)[]

Defined in: [types.ts:758](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L758)

Get the current block of every active scene.

#### Returns

[`Block`](Block.md)[]

***

### getSceneConnections()

> **getSceneConnections**(`sceneId`): [`BlueprintConnection`](../type-aliases/BlueprintConnection.md)[]

Defined in: [types.ts:766](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L766)

Every wire INSIDE a scene, flattened so each carries the block it leaves.

Graph inspection, for a debug view that wants to see the wiring without playing it. It has
never had anything to do with going from one scene to another: a wire has never crossed a
scene in any version of the format, and chaining two scenes is the game's own business.

#### Parameters

##### sceneId

`string`

#### Returns

[`BlueprintConnection`](../type-aliases/BlueprintConnection.md)[]

***

### init()

> **init**(`options`): [`DiagnosticReport`](DiagnosticReport.md)

Defined in: [types.ts:685](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L685)

Validate blueprint data, build internal graph, return diagnostic report.

#### Parameters

##### options

[`InitOptions`](InitOptions.md)

#### Returns

[`DiagnosticReport`](DiagnosticReport.md)

***

### isRunning()

> **isRunning**(): `boolean`

Defined in: [types.ts:754](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L754)

True if at least one scene is active.

#### Returns

`boolean`

***

### onAction()

> **onAction**(`handler`): `void`

Defined in: [types.ts:710](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L710)

Register a global handler for ACTION blocks. The developer MUST handle execution in this handler.

#### Parameters

##### handler

[`ActionHandler`](../type-aliases/ActionHandler.md)

#### Returns

`void`

***

### onBeforeBlock()

> **onBeforeBlock**(`handler`): `void`

Defined in: [types.ts:699](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L699)

Register a handler called before every block. Must call resolve() to continue.

#### Parameters

##### handler

[`BeforeBlockHandler`](../type-aliases/BeforeBlockHandler.md)

#### Returns

`void`

***

### onChoice()

> **onChoice**(`handler`): `void`

Defined in: [types.ts:706](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L706)

Register a global handler for CHOICE blocks. All choices are provided, tagged with `visible` when `onResolveCondition()` is configured.

#### Parameters

##### handler

[`ChoiceHandler`](../type-aliases/ChoiceHandler.md)

#### Returns

`void`

***

### onCondition()

> **onCondition**(`handler`): `void`

Defined in: [types.ts:708](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L708)

Register a global handler for CONDITION blocks. The developer MUST handle evaluation in this handler.

#### Parameters

##### handler

[`ConditionHandler`](../type-aliases/ConditionHandler.md)

#### Returns

`void`

***

### onDialog()

> **onDialog**(`handler`): `void`

Defined in: [types.ts:704](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L704)

Register a global handler for DIALOG blocks. May return a cleanup function.

#### Parameters

##### handler

[`DialogHandler`](../type-aliases/DialogHandler.md)

#### Returns

`void`

***

### onInvalidateBlock()

> **onInvalidateBlock**(`handler`): `void`

Defined in: [types.ts:694](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L694)

Register a handler called when a block fails validation.

#### Parameters

##### handler

[`InvalidateBlockHandler`](../type-aliases/InvalidateBlockHandler.md)

#### Returns

`void`

***

### onResolveCharacter()

> **onResolveCharacter**(`fn`): `void`

Defined in: [types.ts:715](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L715)

Register a global character resolver. Called for every block with `metadata.characters`.

#### Parameters

##### fn

(`characters`) => [`Card`](Card.md) \| `undefined`

#### Returns

`void`

***

### onResolveCondition()

> **onResolveCondition**(`evaluator`): `void`

Defined in: [types.ts:729](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L729)

Install a unified condition evaluator for both choice visibility and condition block pre-evaluation.
The engine handles `choice:` conditions internally via choice history — this callback
evaluates game-state conditions only.

When installed:
- Choice blocks: each choice is tagged with `visible: true | false` based on its `visibilityConditions`.
- Condition blocks: each group is pre-evaluated and the result is available in `context.groups[i].result`
  and `context.evaluation`.

#### Parameters

##### evaluator

(`condition`) => `boolean`

#### Returns

`void`

***

### onSceneEnter()

> **onSceneEnter**(`handler`): `void`

Defined in: [types.ts:735](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L735)

Register a handler called when any scene starts.

#### Parameters

##### handler

[`SceneLifecycleHandler`](../type-aliases/SceneLifecycleHandler.md)

#### Returns

`void`

***

### onSceneExit()

> **onSceneExit**(`handler`): `void`

Defined in: [types.ts:737](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L737)

Register a handler called when any scene ends (natural or cancelled).

#### Parameters

##### handler

[`SceneLifecycleHandler`](../type-aliases/SceneLifecycleHandler.md)

#### Returns

`void`

***

### onValidateNextBlock()

> **onValidateNextBlock**(`handler`): `void`

Defined in: [types.ts:692](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L692)

Register a handler called before each block to validate it.

#### Parameters

##### handler

[`ValidateNextBlockHandler`](../type-aliases/ValidateNextBlockHandler.md)

#### Returns

`void`

***

### scene()

> **scene**(`sceneId`): [`SceneHandle`](SceneHandle.md)

Defined in: [types.ts:742](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L742)

Create a scene handle. Does NOT start the flow — call handle.start().

#### Parameters

##### sceneId

`string`

#### Returns

[`SceneHandle`](SceneHandle.md)

***

### setLocale()

> **setLocale**(`locale`): `void`

Defined in: [types.ts:687](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L687)

Set the active locale for text resolution.

#### Parameters

##### locale

`string`

#### Returns

`void`

***

### stop()

> **stop**(): `void`

Defined in: [types.ts:752](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L752)

Cancel every running scene.

Every one of them, even if a cleanup throws on the way — the first fault surfaces once
there is nothing left to close. A scene opened twice is two scenes here, and both stop.

#### Returns

`void`

================================================================================

[LSDE Dialog Engine](../index.md) / InitOptions

# Interface: InitOptions

Defined in: [types.ts:264](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L264)

Options passed to `engine.init()`.

## Properties

### check?

> `optional` **check?**: [`CheckOptions`](CheckOptions.md)

Defined in: [types.ts:274](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L274)

***

### data

> **data**: [`Blueprints`](Blueprints.md) \| [`Blueprints`](Blueprints.md)[]

Defined in: [types.ts:273](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L273)

One payload, or the several files of a per-scene export.

LSDE can write one file per scene, and each of those is self-contained: it carries the whole
header — every dictionary, function and card — so a scene loads and plays on its own. Pass
the list and the engine stacks the scenes behind one header, after checking that the files
really do come from the same export.

================================================================================

[LSDE Dialog Engine](../index.md) / InvalidateBlockArgs

# Interface: InvalidateBlockArgs

Defined in: [types.ts:532](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L532)

Arguments for the onInvalidateBlock handler.

## Properties

### reason

> **reason**: `string`

Defined in: [types.ts:534](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L534)

***

### scene

> **scene**: [`SceneHandle`](SceneHandle.md)

Defined in: [types.ts:533](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L533)

================================================================================

[LSDE Dialog Engine](../index.md) / Link

# Interface: Link

Defined in: [blueprint-types.ts:138](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L138)

An outgoing wire, seen from the block that carries it.

## Properties

### port

> **port**: `string`

Defined in: [blueprint-types.ts:140](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L140)

The exit port: a fixed port (see Ports), an option id, a case port or an actor id.

***

### to

> **to**: `string`

Defined in: [blueprint-types.ts:142](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L142)

The target block id, relative to the same scene.

***

### toPort

> **toPort**: `string`

Defined in: [blueprint-types.ts:144](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L144)

The target's entry port, always in today.

================================================================================

[LSDE Dialog Engine](../index.md) / NativeProperties

# Interface: NativeProperties

Defined in: [types.ts:90](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L90)

The block properties the ENGINE acts on, read out of `block.props`.

In v2 there is no separate bag: natives and the designer's own properties share `props`, keyed
by bare id. Ids cannot collide — LSDE refuses a project property that takes a native name — so
the only way to tell them apart is this list. [NATIVE\_PROPERTY\_IDS](../variables/NATIVE_PROPERTY_IDS.md) is it.

Most of these are inert: the engine passes `delay`, `timeout`, `debug`, `waitInput`,
`portPerCharacter` and `skipIfMissingActor` through untouched and lets the game decide. Two are
not: `isAsync` opens a parallel track, and `waitForBlocks` holds a block until the ones it
names have FINISHED.

**Inert is not the same as free.** A writer who fills a field in expects a behaviour, and the
doc on each property below says which one. `timeout` is the one that is easy to implement
backwards, so read it before wiring a timer.

**`delay` and `timeout` are MILLISECONDS in v2.** They were seconds in v1, and nothing will
report the difference at runtime — a migrated project turns a 3-second pause into 3 ms.

## Properties

### debug?

> `optional` **debug?**: `boolean`

Defined in: [types.ts:125](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L125)

Editor debug flag. Passed through.

***

### delay?

> `optional` **delay?**: `number`

Defined in: [types.ts:94](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L94)

Milliseconds to wait before the block runs. Applied by `onBeforeBlock`, not by the engine.

***

### inPortPerCharacter?

> `optional` **inPortPerCharacter?**: `boolean`

Defined in: [types.ts:139](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L139)

One ENTRY port per actor id, `in` as the fallback — the mirror of `portPerCharacter`.

The wire names the speaker: a link's `toPort` carries the CARD ID of the actor the block is
to be assigned to on that pass. This is what lets several wires reach one block and each
stand for a different actor — a block alone cannot tell which path brought it.

The engine still ASKS: `onResolveCharacter` is handed that one actor rather than the whole
cast, and a game that returns `undefined` says the character does not exist. Entering
through `in` names nobody, and the callback gets the whole list as everywhere else.

***

### isAsync?

> `optional` **isAsync?**: `boolean`

Defined in: [types.ts:92](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L92)

Run this block on a parallel track instead of the main flow.

***

### portPerCase?

> `optional` **portPerCase?**: `boolean`

Defined in: [types.ts:143](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L143)

Condition blocks: each case exits by its own port instead of sharing `out`.

***

### portPerCharacter?

> `optional` **portPerCharacter?**: `boolean`

Defined in: [types.ts:127](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L127)

One exit port per actor id, `out` as the fallback.

***

### skipIfMissingActor?

> `optional` **skipIfMissingActor?**: `boolean`

Defined in: [types.ts:141](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L141)

Skip the block when its actor is absent at runtime. Passed through.

***

### timeout?

> `optional` **timeout?**: `number`

Defined in: [types.ts:117](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L117)

MILLISECONDS the block STAYS once its line has been said — an auto-advance for blocks.

**The countdown starts at the END of the reveal, not when the block is dispatched.** What
the writer sets is how long the line remains on screen after its last character has been
typed (or its last syllable spoken), and then the block leaves on its own. Counting from
arrival instead cuts the line in half whenever the text takes longer to reveal than the
timeout allows — a 2500 ms timeout on a 120-character line truncates it mid-sentence.

**It overrides [NativeProperties.waitInput](#waitinput) and it overrides leaving immediately.**
All three say WHEN the block is left, and the one the writer put on the card is the most
specific answer. So a click no longer dismisses the block: it may only HURRY the reveal to
its end, which is what arms the countdown. Pressing a line that plays its own time makes no
sense; speeding it up does.

Leaving the block is also what marks it FINISHED, so on a block listed in a
[NativeProperties.waitForBlocks](#waitforblocks) elsewhere, this is the property that releases the
join.

The engine enforces none of it — no timers, no game loop, nothing is read here. The game
arms the countdown, and this is the behaviour the writer is owed when they fill the field.

***

### waitForBlocks?

> `optional` **waitForBlocks?**: `string`[]

Defined in: [types.ts:176](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L176)

Block ids OF THIS SCENE that must have FINISHED before this block STARTS.

The join half of the fork [NativeProperties.isAsync](#isasync) opens: a branch runs in parallel,
and a block downstream waits for it to be over before it plays.

**Finished, not reached.** A listed block counts once the flow has LEFT it: the game called
`next()`, the exit port was resolved, and the block's cleanup has run. So the bubble is off
the screen and the audio voice is stopped before the joining line is dispatched — which is
the whole point of drawing a join.

That is a change from the first v2 releases, where being reached was enough. It made the
property nearly inert in the shape designers actually draw: a fork into two blocks, then a
join on both, lifted in the very tick it was registered because the two had been dispatched
a fraction of a millisecond earlier — and the joining line spoke over them.

**The engine holds the block BEFORE dispatching it.** No handler is called, so the game
never learns the block exists until the wait lifts — nothing of it can reach the screen
early. That is the engine's decision and not a rendering choice a game could make
differently: this is a NATIVE property, the designer ticks it in LSDE, and the engine owes
them the behaviour.

The rule is the same on every track, the one the player is watching included.

- **All** the listed blocks must have finished, not just one.
- Finishing a block releases everything waiting on it, in turn.
- A block that never finishes parks its track for good — and a block that waits for input
  forever never finishes. `init()` reports `UNKNOWN_WAIT_BLOCK` when an id is not a block of
  the scene at all, but it cannot know whether a real one will ever be played.
- `getVisitedBlocks()` is unaffected: it still lists what the player has been SHOWN, which
  includes a block still mid-sentence.

***

### waitInput?

> `optional` **waitInput?**: `boolean`

Defined in: [types.ts:123](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L123)

Wait for player input or a game signal instead of leaving on its own. Passed through, never
interpreted — and outranked by [NativeProperties.timeout](#timeout), which says the block plays
its own time and cannot be dismissed early.

================================================================================

[LSDE Dialog Engine](../index.md) / Option

# Interface: Option

Defined in: [blueprint-types.ts:178](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L178)

One answer of a choice block, a translated key in its own right.

## Extended by

- [`RuntimeChoiceItem`](RuntimeChoiceItem.md)

## Properties

### id

> **id**: `string`

Defined in: [blueprint-types.ts:180](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L180)

The option id, which is also its exit port (C1...).

***

### key

> **key**: `string`

Defined in: [blueprint-types.ts:182](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L182)

The full i18n key of its text.

***

### text?

> `optional` **text?**: [`TextByLocale`](../type-aliases/TextByLocale.md)

Defined in: [blueprint-types.ts:184](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L184)

Its text by locale, when texts are exported.

***

### when?

> `optional` **when?**: [`ConditionTest`](ConditionTest.md)[]

Defined in: [blueprint-types.ts:186](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L186)

Absent = always offered.

================================================================================

[LSDE Dialog Engine](../index.md) / PortResolutionInput

# Interface: PortResolutionInput

Defined in: [types.ts:772](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L772)

What `resolvePort()` needs to pick the wires to follow.

## Properties

### actionRejected?

> `optional` **actionRejected?**: `boolean`

Defined in: [types.ts:792](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L792)

ACTION only: `true` when a call failed, so `catch` is tried before `then`.

***

### actorPort?

> `optional` **actorPort?**: `string`

Defined in: [types.ts:794](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L794)

DIALOG with `portPerCharacter`: the CARD ID of the speaking actor (`var1`), never an index.

***

### block

> **block**: [`Block`](Block.md)

Defined in: [types.ts:774](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L774)

The block being left. Its `type` picks the routing rule.

***

### conditionPort?

> `optional` **conditionPort?**: `string`

Defined in: [types.ts:780](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L780)

CONDITION only: the port its cases picked — `out`, `default`, or `K1`….

***

### links

> **links**: [`Link`](Link.md)[]

Defined in: [types.ts:776](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L776)

The wires it carries — `block.next`, straight off the block.

***

### routerPorts?

> `optional` **routerPorts?**: `string`[]

Defined in: [types.ts:790](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L790)

ROUTER only: every port it leaves by, in order — the `K*` of each true case, then `then` or
`catch` LAST.

A list and not one port, because a router does not pick an exit: it launches one per true
case and continues besides. The continuation comes last so that the traversal, which keeps
the first non-async target as the main flow, keeps `then`/`catch` when the case routes are
async — which is the arrangement LSDE recommends and `init()` warns about otherwise.

***

### selectedOptionId?

> `optional` **selectedOptionId?**: `string`

Defined in: [types.ts:778](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L778)

CHOICE only: the option the player picked. Its id **is** its port (`C1`…).

================================================================================

[LSDE Dialog Engine](../index.md) / PortResolutionResult

# Interface: PortResolutionResult

Defined in: [types.ts:798](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L798)

The wires to follow. The traversal decides which is the main track.

## Properties

### links

> **links**: [`Link`](Link.md)[]

Defined in: [types.ts:799](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L799)

================================================================================

[LSDE Dialog Engine](../index.md) / RouterContext

# Interface: RouterContext

Defined in: [types.ts:375](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L375)

What a ROUTER handler gets.

The same pre-evaluated `cases` as a condition, and **no `resolve`**: a router's exits are a
tally, not a choice. Every true case has already launched its port and the continuation is
already picked — `then` when they all held, `catch` otherwise — by the time a handler could
speak. There is nothing left to override, which is also why no handler is required for the type.

## Extends

- [`BaseBlockContext`](BaseBlockContext.md)

## Properties

### actors

> **actors**: [`Card`](Card.md)[]

Defined in: [types.ts:309](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L309)

Every card the block cites, resolved through the export's `cards` table, in file order.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`actors`](BaseBlockContext.md#actors)

***

### cases

> **cases**: [`RuntimeConditionCase`](RuntimeConditionCase.md)[]

Defined in: [types.ts:377](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L377)

The block's cases, each with its port and its pre-evaluated `result`. ALL of them ran.

***

### character

> **character**: [`Card`](Card.md) \| `undefined`

Defined in: [types.ts:307](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L307)

The actor `onResolveCharacter()` picked for this block, or `undefined`.

A block lists a CAST in `actors` — card ids, in an order LSDE deliberately refuses to give a
meaning to. Whether the first one speaks, whether they all do, whether the rest are simply
present is the game's call, so the engine hands the whole list to `onResolveCharacter()` and
keeps whatever comes back. It does not elect a first one, the way v1 did.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`character`](BaseBlockContext.md#character)

***

### emotion

> **emotion**: [`Card`](Card.md) \| `undefined`

Defined in: [types.ts:317](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L317)

The emotion of the block, resolved through `cards` — the TONE of the line, not of a speaker.

In v1 each character carried its own emotion, which meant writing the same feeling twice for
two actors saying one sentence, and being free to desynchronise them by accident. A block is
one line and one line has one tone; `actors` says who may carry it.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`emotion`](BaseBlockContext.md#emotion)

***

### intensity

> **intensity**: `number` \| `undefined`

Defined in: [types.ts:319](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L319)

How strongly, when the writer set an emotion. Passed through untouched.

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`intensity`](BaseBlockContext.md#intensity)

***

### preventGlobalHandler

> **preventGlobalHandler**: () => `void`

Defined in: [types.ts:321](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L321)

Stop the global (Tier 1) handler from running after this scene handler.

#### Returns

`void`

#### Inherited from

[`BaseBlockContext`](BaseBlockContext.md).[`preventGlobalHandler`](BaseBlockContext.md#preventglobalhandler)

================================================================================

[LSDE Dialog Engine](../index.md) / RuntimeChoiceItem

# Interface: RuntimeChoiceItem

Defined in: [types.ts:196](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L196)

A choice option tagged with what `onResolveCondition()` said about its `when`.

The engine hands over EVERY option, tagged — never a pre-filtered list. Filter with
`options.filter( o => o.visible !== false )`.

## Extends

- [`Option`](Option.md)

## Properties

### id

> **id**: `string`

Defined in: [blueprint-types.ts:180](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L180)

The option id, which is also its exit port (C1...).

#### Inherited from

[`Option`](Option.md).[`id`](Option.md#id)

***

### key

> **key**: `string`

Defined in: [blueprint-types.ts:182](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L182)

The full i18n key of its text.

#### Inherited from

[`Option`](Option.md).[`key`](Option.md#key)

***

### text?

> `optional` **text?**: [`TextByLocale`](../type-aliases/TextByLocale.md)

Defined in: [blueprint-types.ts:184](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L184)

Its text by locale, when texts are exported.

#### Inherited from

[`Option`](Option.md).[`text`](Option.md#text)

***

### visible?

> `optional` **visible?**: `boolean`

Defined in: [types.ts:198](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L198)

`true` = offered, `false` = hidden, `undefined` = no resolver installed (treat as offered).

***

### when?

> `optional` **when?**: [`ConditionTest`](ConditionTest.md)[]

Defined in: [blueprint-types.ts:186](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L186)

Absent = always offered.

#### Inherited from

[`Option`](Option.md).[`when`](Option.md#when)

================================================================================

[LSDE Dialog Engine](../index.md) / RuntimeConditionCase

# Interface: RuntimeConditionCase

Defined in: [types.ts:208](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L208)

A condition case with its pre-evaluated result, set when `onResolveCondition()` is installed.
Mirrors how [RuntimeChoiceItem](RuntimeChoiceItem.md) tags an option.

The case carries its own exit port, so there is no index to map back to anything — that is the
v1 shape (`portIndex`) and it is gone. Pass the `port` to `resolve()` to override the routing.

## Properties

### port

> **port**: `string`

Defined in: [types.ts:210](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L210)

The exit port of this case: `K1`… with `portPerCase`, otherwise the block's `out`.

***

### result?

> `optional` **result?**: `boolean`

Defined in: [types.ts:214](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L214)

`true` if the case holds, `false` if not, `undefined` if no resolver is installed.

***

### when?

> `optional` **when?**: [`ConditionTest`](ConditionTest.md)[]

Defined in: [types.ts:212](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L212)

Its comparisons, chained left to right with no precedence. Absent = always true.

================================================================================

[LSDE Dialog Engine](../index.md) / Scene

# Interface: Scene

Defined in: [blueprint-types.ts:226](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L226)

One scene: its blocks, and where it starts.

## Properties

### blocks

> **blocks**: [`Block`](Block.md)[]

Defined in: [blueprint-types.ts:236](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L236)

Every exported block of the scene.

***

### id

> **id**: `string`

Defined in: [blueprint-types.ts:230](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L230)

The scene identity that SURVIVES A RENAME (sc_ then eight chars). 'scene' is what you read and what builds the i18n keys, but it changes when the writer renames the scene - so an asset that stored it stops resolving, silently. Store THIS one wherever a scene is referenced from outside the payload, and show 'scene' as its label.

***

### label?

> `optional` **label?**: `string`

Defined in: [blueprint-types.ts:232](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L232)

The readable name of the scene, when written.

***

### scene

> **scene**: `string`

Defined in: [blueprint-types.ts:228](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L228)

The scene path without the reserved namespace (acte1, chap1.acte1).

***

### start?

> `optional` **start?**: `string`

Defined in: [blueprint-types.ts:234](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L234)

The entry block id. Absent = the scene has no entry and cannot play.

================================================================================

[LSDE Dialog Engine](../index.md) / SceneContext

# Interface: SceneContext

Defined in: [types.ts:403](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L403)

Context passed to scene lifecycle handlers.

================================================================================

[LSDE Dialog Engine](../index.md) / SceneHandle

# Interface: SceneHandle

Defined in: [types.ts:618](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L618)

Public interface for controlling a running scene.

## Remarks

Obtain a `SceneHandle` by calling `engine.scene(sceneRef)`. Use it to register
scene-specific (Tier 2) handlers, then call `start()` to begin traversal from the
scene's entry block.

**Lifecycle**:
1. `start()` → `onSceneEnter` fires → first block is dispatched
2. Blocks are dispatched sequentially, following connections via port resolution
3. Scene ends when: no more connections, or `cancel()` is called
4. All async tracks are cancelled → current block cleanup runs → `onSceneExit` fires

Scene-level handlers (`onDialog`, `onChoice`, etc.) are called **before** global handlers.
Both tiers execute unless the scene handler calls `context.preventGlobalHandler()`.
Use `onBlock(blockId, handler)` for a block-specific handler that takes highest priority.

## Example

```ts
const handle = engine.scene(sceneId);
handle.onDialog(({ block, context, next }) => {
  showText(block.dialogueText?.['en']);
  next();
});
handle.onExit(({ scene }) => {
  console.log('Scene finished, visited:', scene.getVisitedBlocks().size);
});
handle.start();
```

## See

 - [BlockHandlerArgs](BlockHandlerArgs.md) for handler arguments
 - [BlueprintScene](../type-aliases/BlueprintScene.md) for the scene data structure

## Methods

### cancel()

> **cancel**(): `void`

Defined in: [types.ts:622](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L622)

Cancel the scene flow.

#### Returns

`void`

***

### evaluateCondition()

> **evaluateCondition**(`condition`): `boolean`

Defined in: [types.ts:665](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L665)

Evaluate a condition. Handles `choice:` conditions via internal choice history. Returns `false` for non-choice conditions.

#### Parameters

##### condition

[`ConditionTest`](ConditionTest.md)

#### Returns

`boolean`

***

### getActiveTracks()

> **getActiveTracks**(): `number`

Defined in: [types.ts:655](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L655)

Get the number of async tracks currently running in parallel.

#### Returns

`number`

***

### getChoice()

> **getChoice**(`blockId`): readonly `string`[] \| `undefined`

Defined in: [types.ts:662](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L662)

Get the choice(s) selected at a specific block. Returns undefined if block never visited as choice.

#### Parameters

##### blockId

`string`

#### Returns

readonly `string`[] \| `undefined`

***

### getChoiceHistory()

> **getChoiceHistory**(): `ReadonlyMap`\<`string`, readonly `string`[]\>

Defined in: [types.ts:660](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L660)

Get the full choice history for this scene. Keys are block ids, values are the option ids the player picked there, in order.

#### Returns

`ReadonlyMap`\<`string`, readonly `string`[]\>

***

### getCurrentBlock()

> **getCurrentBlock**(): [`Block`](Block.md) \| `null`

Defined in: [types.ts:649](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L649)

Get the block currently being executed.

#### Returns

[`Block`](Block.md) \| `null`

***

### getTrackInfos()

> **getTrackInfos**(): readonly [`TrackInfo`](TrackInfo.md)[]

Defined in: [types.ts:657](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L657)

Get detailed info for all currently running async tracks. Useful for debug, rendering, and validation.

#### Returns

readonly [`TrackInfo`](TrackInfo.md)[]

***

### getVisitedBlocks()

> **getVisitedBlocks**(): `ReadonlySet`\<`string`\>

Defined in: [types.ts:651](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L651)

The id of every block visited so far, in this scene.

#### Returns

`ReadonlySet`\<`string`\>

***

### isRunning()

> **isRunning**(): `boolean`

Defined in: [types.ts:653](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L653)

Check if the scene flow is currently active.

#### Returns

`boolean`

***

### onAction()

> **onAction**(`handler`): `void`

Defined in: [types.ts:646](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L646)

Override all ACTION blocks for this scene.

#### Parameters

##### handler

[`ActionHandler`](../type-aliases/ActionHandler.md)

#### Returns

`void`

***

### onActionId()

> **onActionId**(`blockId`, `handler`): `void`

Defined in: [types.ts:638](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L638)

Override one ACTION block by its id (type-safe).

#### Parameters

##### blockId

`string`

##### handler

[`ActionHandler`](../type-aliases/ActionHandler.md)

#### Returns

`void`

***

### onBlock()

> **onBlock**(`blockId`, `handler`): `void`

Defined in: [types.ts:630](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L630)

Override one block by its id (DIALOG-001).

#### Parameters

##### blockId

`string`

##### handler

[`BlockHandler`](../type-aliases/BlockHandler.md)\<[`Block`](Block.md), [`BaseBlockContext`](BaseBlockContext.md)\>

#### Returns

`void`

***

### onChoice()

> **onChoice**(`handler`): `void`

Defined in: [types.ts:642](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L642)

Override all CHOICE blocks for this scene.

#### Parameters

##### handler

[`ChoiceHandler`](../type-aliases/ChoiceHandler.md)

#### Returns

`void`

***

### onChoiceId()

> **onChoiceId**(`blockId`, `handler`): `void`

Defined in: [types.ts:634](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L634)

Override one CHOICE block by its id (type-safe).

#### Parameters

##### blockId

`string`

##### handler

[`ChoiceHandler`](../type-aliases/ChoiceHandler.md)

#### Returns

`void`

***

### onCondition()

> **onCondition**(`handler`): `void`

Defined in: [types.ts:644](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L644)

Override all CONDITION blocks for this scene.

#### Parameters

##### handler

[`ConditionHandler`](../type-aliases/ConditionHandler.md)

#### Returns

`void`

***

### onConditionId()

> **onConditionId**(`blockId`, `handler`): `void`

Defined in: [types.ts:636](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L636)

Override one CONDITION block by its id (type-safe).

#### Parameters

##### blockId

`string`

##### handler

[`ConditionHandler`](../type-aliases/ConditionHandler.md)

#### Returns

`void`

***

### onDialog()

> **onDialog**(`handler`): `void`

Defined in: [types.ts:640](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L640)

Override all DIALOG blocks for this scene.

#### Parameters

##### handler

[`DialogHandler`](../type-aliases/DialogHandler.md)

#### Returns

`void`

***

### onDialogId()

> **onDialogId**(`blockId`, `handler`): `void`

Defined in: [types.ts:632](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L632)

Override one DIALOG block by its id (type-safe).

#### Parameters

##### blockId

`string`

##### handler

[`DialogHandler`](../type-aliases/DialogHandler.md)

#### Returns

`void`

***

### onEnter()

> **onEnter**(`handler`): `void`

Defined in: [types.ts:625](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L625)

Override the global onSceneEnter for this scene.

#### Parameters

##### handler

[`SceneLifecycleHandler`](../type-aliases/SceneLifecycleHandler.md)

#### Returns

`void`

***

### onExit()

> **onExit**(`handler`): `void`

Defined in: [types.ts:627](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L627)

Override the global onSceneExit for this scene.

#### Parameters

##### handler

[`SceneLifecycleHandler`](../type-aliases/SceneLifecycleHandler.md)

#### Returns

`void`

***

### onResolveCharacter()

> **onResolveCharacter**(`fn`): `void`

Defined in: [types.ts:667](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L667)

Override character resolution for this scene. Defaults to engine-level resolver.

#### Parameters

##### fn

(`characters`) => [`Card`](Card.md) \| `undefined`

#### Returns

`void`

***

### start()

> **start**(): `void`

Defined in: [types.ts:620](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L620)

Start the scene flow from the entry block.

#### Returns

`void`

================================================================================

[LSDE Dialog Engine](../index.md) / SceneLifecycleArgs

# Interface: SceneLifecycleArgs

Defined in: [types.ts:552](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L552)

Arguments for scene lifecycle handlers.

## Properties

### context

> **context**: [`SceneContext`](SceneContext.md)

Defined in: [types.ts:554](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L554)

***

### scene

> **scene**: [`SceneHandle`](SceneHandle.md)

Defined in: [types.ts:553](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L553)

================================================================================

[LSDE Dialog Engine](../index.md) / TrackInfo

# Interface: TrackInfo

Defined in: [types.ts:569](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L569)

Read-only snapshot of an async track's state.
Returned by [SceneHandle.getTrackInfos](SceneHandle.md#gettrackinfos) for debug, rendering, and validation.

Track IDs are auto-incremented integers starting at 1. The main track is implicit (id 0)
and never appears in the track info list.

## Properties

### currentBlockId

> `readonly` **currentBlockId**: `string` \| `null`

Defined in: [types.ts:577](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L577)

Id of the block currently being processed, or `null` if the track has ended.

***

### id

> `readonly` **id**: `number`

Defined in: [types.ts:571](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L571)

Unique auto-incremented identifier for this track within the scene. Main track is implicit (id 0).

***

### parentTrackId

> `readonly` **parentTrackId**: `number` \| `null`

Defined in: [types.ts:573](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L573)

ID of the track that spawned this one. `null` means spawned directly by the main track.

***

### running

> `readonly` **running**: `boolean`

Defined in: [types.ts:579](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L579)

Whether this track is still actively executing.

***

### startBlockId

> `readonly` **startBlockId**: `string`

Defined in: [types.ts:575](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L575)

Id of the first block that started this track's execution.

================================================================================

[LSDE Dialog Engine](../index.md) / ValidateNextBlockArgs

# Interface: ValidateNextBlockArgs

Defined in: [types.ts:515](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L515)

Arguments for the onValidateNextBlock handler.

## Remarks

Called before each block is executed. Provides the resolved character for both
the upcoming block (`nextContext`) and the previously executed block (`fromContext`).
This enables game-side validation such as character authorization, status checks,
or transition rules between characters.

`fromContext` is `null` for the first block of a scene (no previous block exists).

## Example

```ts
engine.onValidateNextBlock(({ nextBlock, nextContext, fromContext }) => {
  const { character } = nextContext;
  if (!character) return { valid: false, reason: 'no_character' };
  if (game.characterHasStatus(character, 'stunned'))
    return { valid: false, reason: 'character_stunned' };
  return { valid: true };
});
```

## See

 - [ValidateNextBlockContext](ValidateNextBlockContext.md) for per-block context details
 - [Card](Card.md) for character data

## Properties

### fromBlock

> **fromBlock**: [`Block`](Block.md) \| `null`

Defined in: [types.ts:519](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L519)

The block that was just executed, or `null` for the first block of the scene.

***

### fromContext

> **fromContext**: [`ValidateNextBlockContext`](ValidateNextBlockContext.md) \| `null`

Defined in: [types.ts:523](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L523)

Context for the previous block, or `null` if this is the first block.

***

### nextBlock

> **nextBlock**: [`Block`](Block.md)

Defined in: [types.ts:517](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L517)

The block about to be executed.

***

### nextContext

> **nextContext**: [`ValidateNextBlockContext`](ValidateNextBlockContext.md)

Defined in: [types.ts:521](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L521)

Context for the upcoming block (character, etc.).

***

### port

> **port**: `string` \| `null`

Defined in: [types.ts:525](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L525)

The port that was followed to reach `nextBlock` (reserved for future use).

================================================================================

[LSDE Dialog Engine](../index.md) / ValidateNextBlockContext

# Interface: ValidateNextBlockContext

Defined in: [types.ts:485](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L485)

Context attached to a block inside [ValidateNextBlockArgs](ValidateNextBlockArgs.md).

## Remarks

The character is resolved by the `onResolveCharacter` callback **before** the
validation handler is invoked. If the block has no characters in its metadata,
or the resolver returns nothing, `character` will be `undefined`.

## See

 - [Card](Card.md) for character data
 - [ValidateNextBlockArgs](ValidateNextBlockArgs.md) for usage

## Properties

### character

> **character**: [`Card`](Card.md) \| `undefined`

Defined in: [types.ts:487](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L487)

Character resolved for this block, or `undefined` if none.

================================================================================

[LSDE Dialog Engine](../index.md) / ValidationResult

# Interface: ValidationResult

Defined in: [types.ts:278](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L278)

Result of block validation.

## Properties

### reason?

> `optional` **reason?**: `string`

Defined in: [types.ts:289](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L289)

Reason for validation failure. Passed to `InvalidateBlockArgs.reason` when `valid` is `false`.

***

### valid

> **valid**: `boolean`

Defined in: [types.ts:287](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L287)

Whether the block passed validation.

`false` calls `onInvalidateBlock` and then **ends the flow that was entering the block** —
the whole scene when it is the one the player is watching (`onSceneExit` fires), just that
branch when a parallel track was refused. A refusal is a dead end: nothing can resume a
track the game turned away.

================================================================================

[LSDE Dialog Engine](../index.md) / ActionBlock

# Type Alias: ActionBlock

> **ActionBlock** = [`BlockOfType`](BlockOfType.md)\<*typeof* [`Action`](../variables/BlockType.md#action)\>

Defined in: [types.ts:65](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L65)

A call into the game. Carries `calls`, and exits by `then` or `catch`.

================================================================================

[LSDE Dialog Engine](../index.md) / ActionHandler

# Type Alias: ActionHandler

> **ActionHandler** = [`BlockHandler`](BlockHandler.md)\<[`ActionBlock`](ActionBlock.md), [`ActionContext`](../interfaces/ActionContext.md)\>

Defined in: [types.ts:472](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L472)

Handler for ACTION blocks. Shorthand for `BlockHandler<ActionBlock, ActionContext>`.

================================================================================

[LSDE Dialog Engine](../index.md) / BeforeBlockHandler

# Type Alias: BeforeBlockHandler

> **BeforeBlockHandler** = (`args`) => `void`

Defined in: [types.ts:549](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L549)

Handler called before every block. Must call resolve() to continue.

## Parameters

### args

[`BeforeBlockArgs`](../interfaces/BeforeBlockArgs.md)

## Returns

`void`

================================================================================

[LSDE Dialog Engine](../index.md) / BlockHandler

# Type Alias: BlockHandler\<B, C\>

> **BlockHandler**\<`B`, `C`\> = (`args`) => [`CleanupFn`](CleanupFn.md) \| `void`

Defined in: [types.ts:463](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L463)

A block handler function. May return a cleanup function.

## Type Parameters

### B

`B` *extends* [`BlueprintBlock`](BlueprintBlock.md)

### C

`C` *extends* [`BaseBlockContext`](../interfaces/BaseBlockContext.md)

## Parameters

### args

[`BlockHandlerArgs`](../interfaces/BlockHandlerArgs.md)\<`B`, `C`\>

## Returns

[`CleanupFn`](CleanupFn.md) \| `void`

## Remarks

The handler is called when the engine dispatches a block of the matching type. It **must**
call `next()` exactly once to advance the flow to the next block.

If the handler returns a function, it is stored as a **cleanup function** and called when
the engine moves to the next block — use this to tear down UI, stop timers, etc.

## Example

```ts
engine.onDialog(({ block, next }) => {
  const el = showDialogUI(block);
  next();
  return () => el.remove(); // cleanup when leaving this block
});
```

## See

 - [CleanupFn](CleanupFn.md) for the cleanup function type
 - [BlockHandlerArgs](../interfaces/BlockHandlerArgs.md) for handler arguments

================================================================================

[LSDE Dialog Engine](../index.md) / BlockOfType

# Type Alias: BlockOfType\<T\>

> **BlockOfType**\<`T`\> = [`Block`](../interfaces/Block.md) & `object`

Defined in: [types.ts:50](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L50)

A block whose `type` is known.

## Type Declaration

### type

> **type**: `T`

## Type Parameters

### T

`T` *extends* [`BlockType`](BlockType.md)

================================================================================

[LSDE Dialog Engine](../index.md) / BlockType

# Type Alias: BlockType

> **BlockType** = *typeof* [`BlockType`](../variables/BlockType.md)\[keyof *typeof* [`BlockType`](../variables/BlockType.md)\]

Defined in: [blueprint-types.ts:14](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L14)

What a block is. Decides which optional fields it carries, and how the engine reads them. condition and router carry the SAME cases and are read in opposite ways: a condition stops at the first true case and leaves by its port (or by default when none matched); a router evaluates EVERY case, launches one parallel track per true case, then always continues - on then when all cases were true, on catch when any was false. A router with no case at all leaves by then, the way Promise.all([]) resolves.

================================================================================

[LSDE Dialog Engine](../index.md) / BlueprintBlock

# Type Alias: BlueprintBlock

> **BlueprintBlock** = [`Block`](../interfaces/Block.md)

Defined in: [types.ts:26](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L26)

One node of the graph. The engine's name for the generated [Block](../interfaces/Block.md).

================================================================================

[LSDE Dialog Engine](../index.md) / BlueprintConnection

# Type Alias: BlueprintConnection

> **BlueprintConnection** = [`Link`](../interfaces/Link.md) & `object`

Defined in: [types.ts:35](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L35)

A wire seen from OUTSIDE the block that carries it.

In the payload a wire is a [Link](../interfaces/Link.md) listed in `block.next`, so it only knows where it goes —
where it comes from is the block holding it. Graph inspection needs both ends, so the engine
flattens every `next` into this shape. Nothing in the file has it; it exists only in memory.

## Type Declaration

### from

> **from**: `string`

The id of the block this wire leaves, within its scene.

================================================================================

[LSDE Dialog Engine](../index.md) / BlueprintExport

# Type Alias: BlueprintExport

> **BlueprintExport** = [`Blueprints`](../interfaces/Blueprints.md)

Defined in: [types.ts:22](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L22)

A whole export. The engine's name for the generated [Blueprints](../interfaces/Blueprints.md).

================================================================================

[LSDE Dialog Engine](../index.md) / BlueprintScene

# Type Alias: BlueprintScene

> **BlueprintScene** = [`Scene`](../interfaces/Scene.md)

Defined in: [types.ts:24](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L24)

One scene of an export. The engine's name for the generated [Scene](../interfaces/Scene.md).

================================================================================

[LSDE Dialog Engine](../index.md) / CardRole

# Type Alias: CardRole

> **CardRole** = *typeof* [`CardRole`](../variables/CardRole.md)\[keyof *typeof* [`CardRole`](../variables/CardRole.md)\]

Defined in: [blueprint-types.ts:60](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L60)

What a card is used for in the editor.

================================================================================

[LSDE Dialog Engine](../index.md) / ChoiceBlock

# Type Alias: ChoiceBlock

> **ChoiceBlock** = [`BlockOfType`](BlockOfType.md)\<*typeof* [`Choice`](../variables/BlockType.md#choice)\>

Defined in: [types.ts:55](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L55)

A question. Carries `options`, and exits by an option id (`C1`…) — never by `out`.

================================================================================

[LSDE Dialog Engine](../index.md) / ChoiceHandler

# Type Alias: ChoiceHandler

> **ChoiceHandler** = [`BlockHandler`](BlockHandler.md)\<[`ChoiceBlock`](ChoiceBlock.md), [`ChoiceContext`](../interfaces/ChoiceContext.md)\>

Defined in: [types.ts:468](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L468)

Handler for CHOICE blocks. Shorthand for `BlockHandler<ChoiceBlock, ChoiceContext>`.

================================================================================

[LSDE Dialog Engine](../index.md) / CleanupFn

# Type Alias: CleanupFn

> **CleanupFn** = () => `void`

Defined in: [types.ts:293](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L293)

Cleanup function returned by a block handler, called when leaving the block.

## Returns

`void`

================================================================================

[LSDE Dialog Engine](../index.md) / ConditionBlock

# Type Alias: ConditionBlock

> **ConditionBlock** = [`BlockOfType`](BlockOfType.md)\<*typeof* [`Condition`](../variables/BlockType.md#condition)\>

Defined in: [types.ts:57](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L57)

A switch. Carries `cases`, and exits by `out`/`default` or by a case port (`K1`…).

================================================================================

[LSDE Dialog Engine](../index.md) / ConditionEvaluator

# Type Alias: ConditionEvaluator

> **ConditionEvaluator** = (`test`) => `boolean`

Defined in: [condition-evaluator.ts:22](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/condition-evaluator.ts#L22)

What the game answers for one comparison.

## Parameters

### test

[`ConditionTest`](../interfaces/ConditionTest.md)

## Returns

`boolean`

================================================================================

[LSDE Dialog Engine](../index.md) / ConditionHandler

# Type Alias: ConditionHandler

> **ConditionHandler** = [`BlockHandler`](BlockHandler.md)\<[`ConditionBlock`](ConditionBlock.md), [`ConditionContext`](../interfaces/ConditionContext.md)\>

Defined in: [types.ts:470](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L470)

Handler for CONDITION blocks. Shorthand for `BlockHandler<ConditionBlock, ConditionContext>`.

================================================================================

[LSDE Dialog Engine](../index.md) / ConditionJoin

# Type Alias: ConditionJoin

> **ConditionJoin** = *typeof* [`ConditionJoin`](../variables/ConditionJoin.md)\[keyof *typeof* [`ConditionJoin`](../variables/ConditionJoin.md)\]

Defined in: [blueprint-types.ts:36](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L36)

How a comparison links to the one above it. The list is flat: precedence is yours.

================================================================================

[LSDE Dialog Engine](../index.md) / ConditionOperator

# Type Alias: ConditionOperator

> **ConditionOperator** = *typeof* [`ConditionOperator`](../variables/ConditionOperator.md)\[keyof *typeof* [`ConditionOperator`](../variables/ConditionOperator.md)\]

Defined in: [blueprint-types.ts:25](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L25)

How a condition compares a dictionary entry to its value.

================================================================================

[LSDE Dialog Engine](../index.md) / DialogBlock

# Type Alias: DialogBlock

> **DialogBlock** = [`BlockOfType`](BlockOfType.md)\<*typeof* [`Dialog`](../variables/BlockType.md#dialog)\>

Defined in: [types.ts:53](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L53)

A spoken line. Carries `text`, `actors`, `emotion`, and exits by `out`.

================================================================================

[LSDE Dialog Engine](../index.md) / DialogHandler

# Type Alias: DialogHandler

> **DialogHandler** = [`BlockHandler`](BlockHandler.md)\<[`DialogBlock`](DialogBlock.md), [`DialogContext`](../interfaces/DialogContext.md)\>

Defined in: [types.ts:466](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L466)

Handler for DIALOG blocks. Shorthand for `BlockHandler<DialogBlock, DialogContext>`.

================================================================================

[LSDE Dialog Engine](../index.md) / InvalidateBlockHandler

# Type Alias: InvalidateBlockHandler

> **InvalidateBlockHandler** = (`args`) => `void`

Defined in: [types.ts:538](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L538)

Handler called when a block fails validation.

## Parameters

### args

[`InvalidateBlockArgs`](../interfaces/InvalidateBlockArgs.md)

## Returns

`void`

================================================================================

[LSDE Dialog Engine](../index.md) / LiteralValueType

# Type Alias: LiteralValueType

> **LiteralValueType** = *typeof* [`LiteralValueType`](../variables/LiteralValueType.md)\[keyof *typeof* [`LiteralValueType`](../variables/LiteralValueType.md)\]

Defined in: [blueprint-types.ts:52](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L52)

What a dictionary stores, hence what its entries are compared to.

================================================================================

[LSDE Dialog Engine](../index.md) / LocaleTable

# Type Alias: LocaleTable

> **LocaleTable** = `Record`\<`string`, `Record`\<`string`, `string` \| `Record`\<`string`, `string`\>\>\>

Defined in: [lsde-utils.ts:30](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/lsde-utils.ts#L30)

The shape of a `localization/<locale>/__blueprints__.json` file: scene → block, and
scene → block → option for a choice.

This is what an export writes when "Write texts separately" is on — which is the mode most
integrations want. Keeping every locale inline forces a game to load twenty languages to play
one; the split lets it load only the one the player picked, and lets writing and translation
move at their own pace.

================================================================================

[LSDE Dialog Engine](../index.md) / NoteBlock

# Type Alias: NoteBlock

> **NoteBlock** = [`BlockOfType`](BlockOfType.md)\<*typeof* [`Note`](../variables/BlockType.md#note)\>

Defined in: [types.ts:67](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L67)

Designer documentation. Never dispatched — the engine steps over it.

================================================================================

[LSDE Dialog Engine](../index.md) / PropertyBag

# Type Alias: PropertyBag

> **PropertyBag** = `Record`\<`string`, [`PropertyValue`](PropertyValue.md)\>

Defined in: [blueprint-types.ts:89](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L89)

Named property values, keyed by the ids the project declared.

================================================================================

[LSDE Dialog Engine](../index.md) / PropertyValue

# Type Alias: PropertyValue

> **PropertyValue** = `boolean` \| `number` \| `string`

Defined in: [blueprint-types.ts:87](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L87)

What a property, an argument or a condition value can hold.

================================================================================

[LSDE Dialog Engine](../index.md) / RouterBlock

# Type Alias: RouterBlock

> **RouterBlock** = [`BlockOfType`](BlockOfType.md)\<*typeof* [`Router`](../variables/BlockType.md#router)\>

Defined in: [types.ts:63](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L63)

A dispatcher. Carries the SAME `cases` as a condition and reads them the opposite way: every
case is evaluated, each true one launches its port, and the flow then always continues — by
`then` when all of them held, by `catch` when any did not.

================================================================================

[LSDE Dialog Engine](../index.md) / SceneLifecycleHandler

# Type Alias: SceneLifecycleHandler

> **SceneLifecycleHandler** = (`args`) => `void`

Defined in: [types.ts:558](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L558)

Handler for scene enter/exit events.

## Parameters

### args

[`SceneLifecycleArgs`](../interfaces/SceneLifecycleArgs.md)

## Returns

`void`

================================================================================

[LSDE Dialog Engine](../index.md) / TextByLocale

# Type Alias: TextByLocale

> **TextByLocale** = `Record`\<`string`, `string`\>

Defined in: [blueprint-types.ts:85](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L85)

A text by locale code, e.g. `{ en: "Hello", fr: "Bonjour" }`.

================================================================================

[LSDE Dialog Engine](../index.md) / ValidateNextBlockHandler

# Type Alias: ValidateNextBlockHandler

> **ValidateNextBlockHandler** = (`args`) => [`ValidationResult`](../interfaces/ValidationResult.md)

Defined in: [types.ts:529](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/types.ts#L529)

Handler for block validation.

## Parameters

### args

[`ValidateNextBlockArgs`](../interfaces/ValidateNextBlockArgs.md)

## Returns

[`ValidationResult`](../interfaces/ValidationResult.md)

================================================================================

[LSDE Dialog Engine](../index.md) / ValueType

# Type Alias: ValueType

> **ValueType** = *typeof* [`ValueType`](../variables/ValueType.md)\[keyof *typeof* [`ValueType`](../variables/ValueType.md)\]

Defined in: [blueprint-types.ts:43](https://github.com/jonlepage/LS-Dialog-Editor-Engine/blob/9315d107789a97e2f3787efceaa1bb2a7403ab59/lsde-ts/src/blueprint-types.ts#L43)

What a function parameter accepts: a literal, or a key picked in a dictionary.

================================================================================
