package main

import (
	"fmt"

	"golang.org/x/sys/windows/registry"
)

func main() {
	err := registry.DeleteKey(registry.CURRENT_USER, `Software\Mozilla\NativeMessagingHosts\me.onchro`)

	if err != nil {
		fmt.Printf("Error: %s", err)
	} else {
		fmt.Println("Successfully removed the registry entry")
		fmt.Println("You can now delete this folder")
	}
}
