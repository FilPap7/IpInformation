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

let currentPage = 1;
const rowsPerPage = 10;

function getAllCountryInfo() {
    $.ajax({
        url: '/Ip/GetAllCountryWithIpInfo',
        method: 'GET',
        success: function (ipData) {
            renderTable(ipData, currentPage);
            createPaginationControls(ipData);
        },
        error: function () {
            console.error('Error retrieving IP information.');
            alert('Please try again.');
        }
    });
}

function renderTable(data, page) {
    const $tableBody = $('#ip-table-body');
    $tableBody.empty();

    const start = (page - 1) * rowsPerPage;
    const end = start + rowsPerPage;
    const paginatedData = data.slice(start, end);

    paginatedData.forEach(function (ip) {
        const row = `<tr>
                        <td>${ip.ip}</td>
                        <td>${ip.countryName}</td>
                        <td>${ip.twoLetterCode}</td>
                        <td>${ip.threeLetterCode}</td>
                     </tr>`;
        $tableBody.append(row);
    });
}
function createPaginationControls(data) {
    const totalPages = Math.ceil(data.length / rowsPerPage);
    const $paginationControls = $('#pagination-controls');
    $paginationControls.empty();

    // Pagination Wrapper
    const paginationList = $('<ul class="pagination justify-content-center"></ul>');

    // Previous Button
    const prevItem = $('<li class="page-item"><button class="page-link">Previous</button></li>');
    if (currentPage === 1) prevItem.addClass('disabled');
    prevItem.on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            renderTable(data, currentPage);
            createPaginationControls(data);
        }
    });
    paginationList.append(prevItem);

    // Page Number Buttons
    for (let i = 1; i <= totalPages; i++) {
        const pageItem = $(`<li class="page-item"><button class="page-link">${i}</button></li>`);
        if (i === currentPage) pageItem.addClass('active');
        pageItem.on('click', function () {
            currentPage = i;
            renderTable(data, currentPage);
            createPaginationControls(data);
        });
        paginationList.append(pageItem);
    }

    // Next Button
    const nextItem = $('<li class="page-item"><button class="page-link">Next</button></li>');
    if (currentPage === totalPages) nextItem.addClass('disabled');
    nextItem.on('click', function () {
        if (currentPage < totalPages) {
            currentPage++;
            renderTable(data, currentPage);
            createPaginationControls(data);
        }
    });
    paginationList.append(nextItem);

    // Append to controls div
    $paginationControls.append(paginationList);
}