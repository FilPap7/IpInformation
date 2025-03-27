$(document).ready(function () {
    $("form").on("submit", async function (event) {
        event.preventDefault(); // Prevent default form submission

        let username = $("input[name='username']").val();
        let password = $("input[name='password']").val();

        let credentials = {
            userName: username,
            password: password
        };

        try {
            let response = await fetch("/Ip/Login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(credentials)
            });

            if (response.redirected) {
                // If the server redirects, follow the redirect
                var test = decodeURIComponent(response.url);
                window.location.href = decodeURIComponent(response.url);
            } else if (!response.ok) {
                // Handle errors (e.g., incorrect credentials)
                let errorMessage = await response.text();
                alert(errorMessage || "Login failed. Please check your credentials.");
            }
        } catch (error) {
            console.error("Login error:", error);
            alert("An error occurred. Please try again later.");
        }
    });
});