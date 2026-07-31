import { createContext, useContext } from 'react'
import { LATEST_VERSION } from '../versionManifest'

export interface VersionContextValue {
  selectedVersion: string
  setSelectedVersion: (version: string) => void
}

export const VersionContext = createContext<VersionContextValue>({
  selectedVersion: LATEST_VERSION.version,
  setSelectedVersion: () => {},
})

export const useSelectedVersion = () => useContext(VersionContext)
