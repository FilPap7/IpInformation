// Function to get country information for a specific IP address
async function getCountryInfo() {

    const ipInput = document.getElementById('ipInput').value;
    const ipInfoDiv = document.getElementById('ipInfo'); // Ensure this ID matches HTML
    ipInfoDiv.innerHTML = ''; // Clear previous result

    if (!ipInput) {
        ipInfoDiv.textContent = 'Please enter a valid IP address.';
        return;
    }

    try {
        // Send a POST request to the API with the IP address in the request body
        const response = await fetch('/Ip', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(ipInput)
        });

        if (!response.ok) {
            throw new Error('Failed to fetch country information');
        }

        const country = await response.json();

        // Display country information
        const infoHtml = `<h3>Country Information</h3>
                          <p><strong>ID:</strong> ${country.id}</p>
                          <p><strong>Name:</strong> ${country.name}</p>
                          <p><strong>Two-Letter Code:</strong> ${country.twoLetterCode}</p>
                          <p><strong>Three-Letter Code:</strong> ${country.threeLetterCode}</p>
                          <p><strong>Created At:</strong> ${new Date(country.createdAt).toLocaleString()}</p>`;
        ipInfoDiv.innerHTML = infoHtml;
    } catch (error) {
        console.error('There was an error retrieving country information:', error);
        ipInfoDiv.textContent = 'Error retrieving country information. Please try again.';
    }
}

async function getAllCountryInfo() {
    try {
        // Fetch all stored IP information from your API
        const response = await fetch('/Ip/GetAllCountryWithIpInfo', {
            method: 'Get'
        });

        if (!response.ok) {
            throw new Error('Failed to fetch stored IP information');
        }

        const ipData = await response.json();

        // Get the table body element
        const tableBody = document.getElementById('ip-table-body');
        tableBody.innerHTML = ''; // Clear any existing rows

        // Populate the table with the fetched IP data
        ipData.forEach(ip => {
            // Fetch country information for each IP
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${ip.ip}</td> <!-- Display IP -->
                <td>${ip.countryName}</td> <!-- Display Country Name -->
                <td>${ip.twoLetterCode}</td> <!-- Display Two-Letter Code -->
                <td>${ip.threeLetterCode}</td> <!-- Display Three-Letter Code -->
            `;
            tableBody.appendChild(row);
        });
    } catch (error) {
        console.error('There was an error retrieving stored IP information:', error);
        alert('Error retrieving stored IP information. Please try again.');
    }
}

