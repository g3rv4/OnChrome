package extensionendpoint

import "os/exec"

func openChrome(url string) error {
	cmd := exec.Command("open", "-a", "Google Chrome", url)
	return cmd.Run()
}
