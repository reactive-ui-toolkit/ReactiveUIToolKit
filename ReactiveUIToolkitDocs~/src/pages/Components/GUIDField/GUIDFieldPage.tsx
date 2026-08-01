import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './GUIDFieldPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { GUID_FIELD_BASIC, GUID_FIELD_READONLY } from './GUIDFieldPage.example'

export const GUIDFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      GUIDField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.GUIDField</code> is a text field for a <code>UnityEngine.GUID</code> value. It is
      available from <strong>Unity 6.4</strong>.
    </Typography>
    <Typography variant="body1" paragraph>
      Despite the name, this is a <em>runtime</em> control: its value type is{' '}
      <code>UnityEngine.GUID</code>, not <code>UnityEditor.GUID</code>, so it works in player builds
      as well as in the editor.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('GUIDFieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={GUID_FIELD_BASIC} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Read-only and delayed input
      </Typography>
      <Typography variant="body1" paragraph>
        <code>GUIDField</code> inherits the text-input behaviours, so <code>readOnly</code>,{' '}
        <code>isDelayed</code>, <code>maxLength</code> and <code>selectAllOnFocus</code> all apply.
        The value accepts either a <code>GUID</code> or a dashed/undashed hex string, matching how
        UXML round-trips it.
      </Typography>
      <CodeBlock language="jsx" code={GUID_FIELD_READONLY} />
    </Box>
    <UnityDocsSection componentName="GUIDField" />
  </Box>
)
