# Branch rulesets

The canonical pair of GitHub branch rulesets for every repo in the Reactive UI
Toolkit family. GitHub does not read this folder automatically — apply them via
**Settings → Rules → Rulesets → Import a ruleset**, once per repo.

- `protect-dev.json` — `dev` advances only through PRs (merge commits), gated on
  the four required status checks `gates`, `tests`, `extensions`, `docs`. Those
  context names are load-bearing: they must match the job `name:` fields in
  `.github/workflows/test.yml` exactly, or the ruleset waits forever for a check
  that never reports.
- `protect-master.json` — `master` is the release pointer: no deletion, no
  force-push. It advances by fast-forwarding to `dev`; no PR/check gate of its
  own (everything on it already passed the `dev` gate).

Edit the JSON here first, re-import, and keep the family repos in sync — the
files are the source of truth, the GitHub UI state is a copy.
