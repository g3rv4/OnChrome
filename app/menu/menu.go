package menu

import (
	"fmt"
	"io"
	"os"
	"path/filepath"
	"strings"

	"github.com/eiannone/keyboard"
)

// ShowMenu shows the menu
func ShowMenu() {
	err := keyboard.Open()
	if err != nil {
		panic(err)
	}
	defer keyboard.Close()

	clearTerminal()
	for {
		fmt.Println("What do you want to do?")
		fmt.Println("1. Register native application")
		fmt.Println("2. Unregister native application")
		fmt.Println()
		fmt.Println("Press ESC to exit")
		fmt.Println()

		char, key, err := keyboard.GetKey()
		if err != nil {
			panic(err)
		} else if key == keyboard.KeyEsc {
			os.Exit(0)
		}

		switch char {
		case '1':
			register()
		case '2':
			unregister()
		default:
			fmt.Println(">> Unrecognized option.")
			fmt.Println()
			fmt.Println()
			continue
		}
		os.Exit(0)
	}
}

func register() {
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
	exPath, err := getExecutablePath(basePath)
	if err != nil {
		return "", err
	}

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
