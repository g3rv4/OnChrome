package extensionendpoint

import (
	"io"
	"os"
	"runtime"

	"github.com/qrtz/nativemessaging"
)

// AnswerToExtension receives the input from the extension from stdin and writes the answer back
func AnswerToExtension() {
	decoder := nativemessaging.NewNativeJSONDecoder(os.Stdin)
	encoder := nativemessaging.NewNativeJSONEncoder(os.Stdout)

	var rsp response
	var msg message
	err := decoder.Decode(&msg)

	if err != nil {
		if err == io.EOF {
			// exit
			return
		}
		rsp.Text = err.Error()
	} else {
		rsp.Success = true

		if runtime.GOOS != "darwin" && runtime.GOOS != "windows" {
			rsp.Success = false
			rsp.Text = "Unknown OS"
		} else {
			rsp.Text = "ok"
			err = openChrome(msg.Profile, msg.Url)

			if err != nil {
				rsp.Success = false
				rsp.Text = "Command finished with error" + err.Error()
			}
		}
	}

	if err := encoder.Encode(rsp); err != nil {
		// Log the error and exit
		return
	}
}

type message struct {
	Profile string `json:"profile"`
	Url     string `json:"url"`
}

type response struct {
	Text    string `json:"text"`
	Success bool   `json:"success"`
}
