var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("Inprocess")) {
        LoadDataTable("GetOrderList?status=Inprocess");
    }
    else {
        if (url.includes("Shipped")) {
            LoadDataTable("GetOrderList?status=Shipped");
        }
        else {
            if (url.includes("Completed")) {
                LoadDataTable("GetOrderList?status=Completed");
            }
            else {
                if (url.includes("Rejected")) {
                    LoadDataTable("GetOrderList?status=Rejected");
                }
                else {
                    LoadDataTable("GetOrderList?status=All");
                }
            }
        }
    }
  });

function LoadDataTable(url) {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Order/" + url
        },
        "columns": [
            { "data": "id", "Width": "10%" },
            { "data": "name", "Width": "10%" },
            { "data": "phoneNuber", "Width": "10%" },
            { "data": "applicationUser.email", "Width": "10%" },
            { "data": "orderStatus", "Width": "10%" },
            { "data": "orderTotal", "Width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                               <a href="/Admin/Order/Details/${data}" class="btn btn-success text-white"><i class="fas fa-edit"></i></a>
                            </div>`;
                }, "width": "5%"
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
