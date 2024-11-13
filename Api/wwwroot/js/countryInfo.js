// Function to get country information for a specific IP address
function getCountryInfo() {
    const ipInput = $('#ipInput').val();
    const $ipInfoDiv = $('#ipInfo'); // Ensure this ID matches HTML
    $ipInfoDiv.empty(); // Clear previous result

    if (!ipInput) {
        $ipInfoDiv.text('Please enter a valid IP address.');
        return;
    }

    $.ajax({
        url: '/Ip',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(ipInput),
        success: function (country) {
            const infoHtml = `<h3>Country Information</h3>
                              <p><strong>ID:</strong> ${country.id}</p>
                              <p><strong>Name:</strong> ${country.name}</p>
                              <p><strong>Two-Letter Code:</strong> ${country.twoLetterCode}</p>
                              <p><strong>Three-Letter Code:</strong> ${country.threeLetterCode}</p>
                              <p><strong>Created At:</strong> ${new Date(country.createdAt).toLocaleString()}</p>`;
            $ipInfoDiv.html(infoHtml);
        },
        error: function () {
            console.error('There was an error retrieving country information.');
            $ipInfoDiv.text('Error retrieving country information. Please try again.');
        }
    });
}

// Function to get all country information
function getAllCountryInfo() {
    $.ajax({
        url: '/Ip/GetAllCountryWithIpInfo',
        method: 'GET',
        success: function (ipData) {
            const $tableBody = $('#ip-table-body');
            $tableBody.empty(); // Clear any existing rows

            ipData.forEach(function (ip) {
                const row = `<tr>
                                <td>${ip.ip}</td>
                                <td>${ip.countryName}</td>
                                <td>${ip.twoLetterCode}</td>
                                <td>${ip.threeLetterCode}</td>
                             </tr>`;
                $tableBody.append(row);
            });
        },
        error: function () {
            console.error('There was an error retrieving stored IP information.');
            alert('Error retrieving stored IP information. Please try again.');
        }
    });
}