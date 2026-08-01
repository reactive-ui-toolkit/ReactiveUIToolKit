export const MASK64_FIELD_BASIC = `// Example namespace: Ruitk.Samples.Components
// Requires Unity 6.5 or newer.

VirtualNode Mask64FieldExample() {
  var (mask, setMask) = useState(0UL);

  return (
    <Mask64Field
      labelText="Capabilities"
      choices={new List<string> { "Read", "Write", "Execute" }}
      value={mask}
      onChange={evt => setMask(evt.newValue)}
    />
  );
}`

export const MASK64_FIELD_WIDE = `// The 64-bit sibling of MaskField: use it when you need more than
// 32 independent flags. choicesMasks is List<ulong> and the default
// per-choice mask is 1UL << i.

<Mask64Field
  labelText="Feature flags"
  choices={flagNames}
  choicesMasks={new List<ulong> { 1UL << 0, 1UL << 33, 1UL << 63 }}
  value={mask}
  onChange={evt => setMask(evt.newValue)}
/>`
