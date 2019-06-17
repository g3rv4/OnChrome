package menu

import (
	"fmt"
	"io"
	"os"
	"os/exec"
	"path/filepath"
	"strings"

	"golang.org/x/sys/windows/registry"
)

func getExecutablePath(currentPath string) (string, error) {
	path := currentPath + "\\FirefoxEndpoint.bat"

	// create the bat file
	fo, err := os.Create(path)
	if err != nil {
		return "", err
	}
	defer fo.Close()

	_, err = io.Copy(fo, strings.NewReader("@echo off\nOnChrome.exe ff"))
	if err != nil {
		return "", err
	}

	return path, nil
}

func getManifestPath(currentPath string) (string, error) {
	return currentPath + "\\manifest.json", nil
}

func registerManifest(manifestPath string) error {
	k, _, err := registry.CreateKey(registry.CURRENT_USER, `Software\Mozilla\NativeMessagingHosts\me.onchro`, registry.ALL_ACCESS)

	if err != nil {
		return err
	}
	defer k.Close()

	return k.SetStringValue("", manifestPath)
}

func clearTerminal() {
	cmd := exec.Command("cmd", "/c", "cls")
	cmd.Stdout = os.Stdout
	cmd.Run()
}

func unregister() {
	fmt.Println("Deleting key from registry")
	err := registry.DeleteKey(registry.CURRENT_USER, `Software\Mozilla\NativeMessagingHosts\me.onchro`)

	if err != nil {
		fmt.Printf("Error: %s", err)
		return
	}

	ex, err := os.Executable()
	if err != nil {
		fmt.Printf("Error: %s", err)
		return
	}

	basePath := filepath.Dir(ex)

	manifestPath, err := getManifestPath(basePath)
	if err != nil {
		fmt.Printf("Error: %s", err)
		return
	}

	fmt.Printf("Deleting manifest at %s\n", manifestPath)
	err = os.Remove(manifestPath)
	if err != nil {
		fmt.Printf("Error: %s", err)
		return
	}

	batPath := basePath + "\\FirefoxEndpoint.bat"
	if _, err := os.Stat(batPath); !os.IsNotExist(err) {
		// bat exists
		fmt.Printf("Deleting bat file at %s\n", batPath)
		err = os.Remove(batPath)
		if err != nil {
			fmt.Printf("Error: %s", err)
			return
		}
	}

	if err != nil {
		fmt.Printf("Error: %s", err)
	} else {
		fmt.Println("Successfully removed the registry entry")
		fmt.Println("You can now delete this application")
	}
}
