package manageapp

import (
	"fmt"
	"io"
	"os"
	"path/filepath"
	"strings"
)

// Register registers the app to receive native messages
func Register() {
	fmt.Println("Saving manifest...")

	manifestPath, err := saveManifest()

	if err != nil {
		fmt.Printf("Error %s\n", err)
		return
	}

	fmt.Printf("Saved to %s\n", manifestPath)
	err = registerManifest(manifestPath)

	if err != nil {
		fmt.Printf("Error %s", err)
		return
	}

	fmt.Println("Done!")
}

func saveManifest() (string, error) {
	var manifest = `{
  "name": "me.onchro",
  "description": "Extension to open certain urls on chrome",
  "path": "pathToExecutable",
  "type": "stdio",
  "allowed_extensions": [ "onchrome@gervas.io" ]
}`

	ex, err := os.Executable()
	if err != nil {
		return "", err
	}

	basePath := filepath.Dir(ex)
	exPath := getExecutablePath(basePath)

	// escape it for the json file
	exPath = strings.ReplaceAll(exPath, "\\", "\\\\")

	manifest = strings.Replace(manifest, "pathToExecutable", exPath, 1)

	manifestPath, err := getManifestPath(basePath)
	if err != nil {
		return "", err
	}

	fo, err := os.Create(manifestPath)
	if err != nil {
		return "", err
	}
	defer fo.Close()

	_, err = io.Copy(fo, strings.NewReader(manifest))
	if err != nil {
		return "", err
	}

	return manifestPath, nil
}
