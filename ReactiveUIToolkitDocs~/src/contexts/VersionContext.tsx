import { useState, useCallback, type FC, type ReactNode } from 'react'
import { LATEST_VERSION, SUPPORTED_VERSIONS } from '../versionManifest'
import { VersionContext } from './useSelectedVersion'

const STORAGE_KEY = 'ruitk-selected-unity-version'

function loadPersistedVersion(): string {
  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    if (stored && SUPPORTED_VERSIONS.some((v) => v.version === stored)) return stored
  } catch { /* SSR / private browsing */ }
  return LATEST_VERSION.version
}

export const VersionProvider: FC<{ children: ReactNode }> = ({ children }) => {
  const [selectedVersion, setRaw] = useState(loadPersistedVersion)

  const setSelectedVersion = useCallback((version: string) => {
    setRaw(version)
    try { localStorage.setItem(STORAGE_KEY, version) } catch { /* ignore */ }
  }, [])

  return (
    <VersionContext.Provider value={{ selectedVersion, setSelectedVersion }}>
      {children}
    </VersionContext.Provider>
  )
}
