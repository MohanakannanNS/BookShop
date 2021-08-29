var dataTable;

$(document).ready(function () {
    LoadDataTable();
});

function LoadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Company/GetAll"
        },
        "columns": [
            { "data": "name", "Width": "15%" },
            { "data": "streetAddress", "Width": "15%" },
            { "data": "city", "Width": "10%" },
            { "data": "state", "Width": "10%" },
            { "data": "phoneNumber", "Width": "15%" },
            {
                "data": "isAuthorizedCompany",
                "render": function (data) {
                    if (data) {
                        return `<input type="checkbox" disabled checked>`
                    }
                    else {
                        return `<input type="checkbox" disabled >`
                    }
                },
                "Width": "15%"
            },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                               <a href="/Admin/Company/Upsert/${data}" class="btn btn-success text-white"><i class="fas fa-edit"></i></a>
                                <a onclick=Delete("/Admin/Company/Delete/${data}") class="btn btn-danger text-white"><i class="fas fa-trash"></i></a>
                            </div>`;
                }, "width": "20%"
            }
        ]

    });
}

    function Delete(url) {
        swal({
            title: "Are you sure you want to Delete?",
            text: "You will not be able to retrive data",
            icon: "warning",
            buttons: true,
            dangerMode: true
        }).then((willDelete) => {
            if (willDelete) {
                $.ajax({
                    url: url,
                    type: "DELETE",
                    success: function (data) {
                        if (data.success) {
                            toastr.success(data.message);
                            dataTable.ajax.reload();
                        }
                        else {
                            toastr.error(data.message);
                        }
                    }
                });
            }
        }
            );
    }
