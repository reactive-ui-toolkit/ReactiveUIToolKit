export const MASK_FIELD_BASIC = `// Example namespace: Ruitk.Samples.Components
// Requires Unity 6.5 or newer.

VirtualNode MaskFieldExample() {
  var (mask, setMask) = useState(0);

  return (
    <MaskField
      labelText="Layers"
      choices={new List<string> { "Ground", "Player", "Enemy" }}
      value={mask}
      onChange={evt => setMask(evt.newValue)}
    />
  );
}`

export const MASK_FIELD_COMPOSITE = `// choicesMasks overrides the default 1 << i per entry,
// so a single choice can represent a composite of several bits.

<MaskField
  labelText="Presets"
  choices={new List<string> { "Ground", "Characters", "All gameplay" }}
  choicesMasks={new List<int> { 1, 2 | 4, 1 | 2 | 4 }}
  value={mask}
  onChange={evt => setMask(evt.newValue)}
/>`

export const MASK_FIELD_SENTINELS = `// The dropdown always prefixes two synthetic entries:
//   "Nothing"    -> value 0
//   "Everything" -> value ~0  (that is -1, NOT (1 << n) - 1)
//
// Those are different values. Do not normalise one into the other when you
// persist or compare a mask, or "Everything" silently degrades into "every
// bit that happened to be defined at the time".

if (mask == ~0) {
  // user picked Everything
} else if (mask == 0) {
  // user picked Nothing
}`
