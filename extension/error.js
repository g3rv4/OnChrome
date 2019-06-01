var url = new URL(document.URL);
var error = url.searchParams.get("error");
document.getElementById('error').innerText = error;
