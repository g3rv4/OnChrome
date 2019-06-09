package main

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"

	"../manageapp"
	tm "github.com/buger/goterm"
	"github.com/eiannone/keyboard"
)

func main() {
	for {
		// Gatekeeper copies the app when running it from the Downloads folder
		// that causes us not to have a path to FirefoxEndpoint. We can't then
		// register on unregister it when that happens
		ex, err := os.Executable()
		if err != nil {
			panic(err)
		}
		basePath := filepath.Dir(ex)
		if strings.Contains(basePath, "AppTranslocation") {
			fmt.Println("This app can't be run from the Downloads folder. Please move it and try again")
			os.Exit(1)
		}

		err = keyboard.Open()
		if err != nil {
			panic(err)
		}
		defer keyboard.Close()

		tm.Clear()
		for {
			tm.MoveCursor(1, 1)
			tm.Flush()

			fmt.Println("What do you want to do?")
			fmt.Println("1. Register native application")
			fmt.Println("2. Unregister native application")
			fmt.Println()
			fmt.Println("Press ESC to quit")
			fmt.Println()

			char, key, err := keyboard.GetKey()
			if err != nil {
				panic(err)
			} else if key == keyboard.KeyEsc {
				os.Exit(0)
			}

			switch char {
			case '1':
				manageapp.Register()
			case '2':
				manageapp.Unregister()
			default:
				fmt.Println("Unrecognized option.")
				continue
			}
			os.Exit(0)
		}
	}
}
