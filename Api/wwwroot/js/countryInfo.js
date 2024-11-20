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

let currentContinuationToken = ""; // Tracks the current token
let previousTokens = []; // Stack to track previous tokens for "Previous" functionality
const rowsPerPage = 10;

function getAllCountryInfo() {
    const requestBody = {
        take: rowsPerPage,
        continuationToken: currentContinuationToken
    };

    $.ajax({
        url: '/Ip/GetAllBundledInfo',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(requestBody),
        success: function (response) {
            const { countryWithIp, continuationToken } = response;
            renderTable(countryWithIp);
            updatePaginationState(continuationToken);
        },
        error: function () {
            console.error('Error retrieving IP information.');
            alert('Failed to fetch data. Please try again.');
        }
    });
}

function renderTable(data) {
    const $tableBody = $('#ip-table-body');
    $tableBody.empty();

    data.forEach(function (ip) {
        const row = `<tr>
                        <td>${ip.ip}</td>
                        <td>${ip.countryName}</td>
                        <td>${ip.twoLetterCode}</td>
                        <td>${ip.threeLetterCode}</td>
                     </tr>`;
        $tableBody.append(row);
    });
}

function updatePaginationState(newToken) {
    const $paginationControls = $('#pagination-controls');
    $paginationControls.empty();

    const paginationList = $('<ul class="pagination justify-content-center"></ul>');

    // Previous Button
    const prevItem = $('<li class="page-item"><button class="page-link">Previous</button></li>');
    if (previousTokens.length === 0) prevItem.addClass('disabled');
    prevItem.on('click', function () {
        if (previousTokens.length > 0) {
            currentContinuationToken = previousTokens.pop(); // Go to the last continuation token
            getAllCountryInfo();
        }
    });
    paginationList.append(prevItem);

    // Next Button
    const nextItem = $('<li class="page-item"><button class="page-link">Next</button></li>');
    if (!newToken) nextItem.addClass('disabled');
    nextItem.on('click', function () {
        if (newToken) {
            previousTokens.push(currentContinuationToken); // Save current token for "Previous"
            currentContinuationToken = newToken; // Set new token for "Next"
            getAllCountryInfo();
        }
    });
    paginationList.append(nextItem);

    $paginationControls.append(paginationList);
}
