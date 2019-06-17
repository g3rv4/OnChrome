package main

import (
	"os"

	"../extensionendpoint"
	"../menu"
)

func main() {
	isFirefox := false
	for _, arg := range os.Args[1:] {
		if arg == "ff" {
			isFirefox = true
			break
		}
	}

	if isFirefox {
		extensionendpoint.AnswerToExtension()
	} else {
		menu.ShowMenu()
	}
}
