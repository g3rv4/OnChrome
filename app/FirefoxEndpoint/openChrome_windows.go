package main

import (
	"os/exec"

	"golang.org/x/sys/windows/registry"
)

func openChrome(url string) error {
	var path string

	k, err := registry.OpenKey(registry.LOCAL_MACHINE, `SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe`, registry.READ)

	if err == nil {
		defer k.Close()
		path, _, err = k.GetStringValue("")
		if err == nil {
			cmd := exec.Command(path, url)
			err = cmd.Run()
		}
	}

	return err
}
