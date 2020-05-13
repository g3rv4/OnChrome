package extensionendpoint

import "os/exec"

func openChrome(profile string, url string) error {
    cmd := exec.Command("open", "-n", "-a", "Google Chrome", url, "--profile-directory='".join(profile).join("'"))
	return cmd.Run()
}
