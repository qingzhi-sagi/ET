# ET.Harness

`cn.etetet.harness` packages the AI Harness skills used by this ET project so they can be distributed with the normal ET package workflow.

## Contents

- `skills/index.md`: skill routing index.
- `skills/*.md`: lightweight skill entries.
- `skills/references/*.md`: detailed rules and command references.

## Install Notes

After installing this package into another ET project, point that project's root `AGENTS.md` to:

```text
Packages/cn.etetet.harness/skills/index.md
```

If the target project expects `./skills/index.md`, copy or sync `Packages/cn.etetet.harness/skills` to the project root.
