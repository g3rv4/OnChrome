package main

import (
	"fmt"
	"os"
	"os/user"
)

func main() {
	usr, err := user.Current()
	if err != nil {
		fmt.Printf("Error: %s", err)
		return
	}

	manifestPath := usr.HomeDir + "/Library/Application Support/Mozilla/NativeMessagingHosts/me.onchro.json"

	err = os.Remove(manifestPath)
	if err != nil {
		fmt.Printf("Error: %s", err)
	} else {
		fmt.Printf("Successfully deleted %s\n", manifestPath)
		fmt.Println("You can now delete this folder")
	}
}
