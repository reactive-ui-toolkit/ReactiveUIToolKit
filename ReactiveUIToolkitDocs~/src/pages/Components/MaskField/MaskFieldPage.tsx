import type { FC } from 'react'
import { Alert, Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './MaskFieldPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import {
  MASK_FIELD_BASIC,
  MASK_FIELD_COMPOSITE,
  MASK_FIELD_SENTINELS,
} from './MaskFieldPage.example'

export const MaskFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      MaskField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.MaskField</code> is a multi-select bitmask dropdown backed by an <code>int</code>. It
      is available from <strong>Unity 6.5</strong>.
    </Typography>
    <Typography variant="body1" paragraph>
      <code>MaskField</code> existed before 6.5, but only in <code>UnityEditor.UIElements</code> as
      an editor-only control. Unity 6.5 moved it into the runtime module, which is what makes it
      usable here.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('MaskFieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={MASK_FIELD_BASIC} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Composite masks
      </Typography>
      <Typography variant="body1" paragraph>
        By default each entry in <code>choices</code> maps to <code>1 &lt;&lt; i</code>. Supply{' '}
        <code>choicesMasks</code> to override that per entry, which lets a single choice stand for a
        combination of bits.
      </Typography>
      <CodeBlock language="jsx" code={MASK_FIELD_COMPOSITE} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        &quot;Nothing&quot; and &quot;Everything&quot;
      </Typography>
      <Alert severity="warning" sx={{ mb: 2 }}>
        <strong>Everything is <code>~0</code> (that is <code>-1</code>), not{' '}
        <code>(1 &lt;&lt; n) - 1</code>.</strong> Never normalise the two into each other. A user who
        picked &quot;Everything&quot; and a user who ticked every currently-defined bit are different
        values, and collapsing them loses that distinction the next time the list of choices grows.
      </Alert>
      <CodeBlock language="jsx" code={MASK_FIELD_SENTINELS} />
    </Box>
    <UnityDocsSection componentName="MaskField" />
  </Box>
)
