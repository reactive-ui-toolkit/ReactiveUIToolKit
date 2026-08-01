import type { FC } from 'react'
import { Alert, Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './Mask64FieldPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { MASK64_FIELD_BASIC, MASK64_FIELD_WIDE } from './Mask64FieldPage.example'

export const Mask64FieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Mask64Field
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Mask64Field</code> is the 64-bit sibling of <code>V.MaskField</code>, backed by a{' '}
      <code>ulong</code>. It is available from <strong>Unity 6.5</strong>.
    </Typography>
    <Typography variant="body1" paragraph>
      Reach for it when you need more than 32 independent flags. Everything else about it matches{' '}
      <code>MaskField</code>: the same choices, the same composite-mask override, and the same two
      synthetic entries at the top of the dropdown.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('Mask64FieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={MASK64_FIELD_BASIC} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Wide flag sets
      </Typography>
      <Typography variant="body1" paragraph>
        <code>choicesMasks</code> is a <code>List&lt;ulong&gt;</code> here, and the default
        per-choice mask is <code>1UL &lt;&lt; i</code>.
      </Typography>
      <CodeBlock language="jsx" code={MASK64_FIELD_WIDE} />
    </Box>
    <Box sx={Styles.section}>
      <Alert severity="warning">
        As with <code>MaskField</code>, &quot;Everything&quot; is <code>~0UL</code> and
        &quot;Nothing&quot; is <code>0</code>. Keep them distinct from an explicit all-bits value.
      </Alert>
    </Box>
    <UnityDocsSection componentName="Mask64Field" />
  </Box>
)
