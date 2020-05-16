package extensionendpoint

import "os/exec"

func openChrome(profile string, url string) error {
	var cmd *exec.Cmd

	if len(profile) > 0 {
		cmd = exec.Command("open", "-a", "Google Chrome", "-n", "--args", "--profile-directory="+profile, url)
	} else {
		cmd = exec.Command("open", "-a", "Google Chrome", url)
	}
	return cmd.Run()
}
