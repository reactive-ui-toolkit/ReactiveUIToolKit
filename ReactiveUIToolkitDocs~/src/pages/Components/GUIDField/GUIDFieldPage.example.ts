export const GUID_FIELD_BASIC = `// Example namespace: Ruitk.Samples.Components
// Requires Unity 6.4 or newer.

VirtualNode GUIDFieldExample() {
  var (id, setId) = useState(UnityEngine.GUID.Generate());

  return (
    <GUIDField
      labelText="Asset id"
      value={id}
      onChange={evt => setId(evt.newValue)}
    />
  );
}`

export const GUID_FIELD_READONLY = `// A read-only, delayed field that commits on Enter or blur
// instead of on every keystroke.

<GUIDField
  labelText="Locked id"
  value={id}
  readOnly={true}
  isDelayed={true}
  selectAllOnFocus={true}
/>`
