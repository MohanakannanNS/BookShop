var dataTable;

$(document).ready(function () {
    LoadDataTable();
});

function LoadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/User/GetAll"
        },
        "columns": [
            { "data": "name", "Width": "15%" },
            { "data": "email", "Width": "15%" },
            { "data": "phoneNumber", "Width": "15%" },
            { "data": "company.name", "Width": "15%" },
            { "data": "role", "Width": "15%" },
            {
                "data": {
                    id: "id", lockoutEnd: "lockoutEnd"
                },
                    "render": function (data) {
                        var today = new Date().getTime();
                        var lockout = new Date(data.lockoutEnd).getTime();
                        if (lockout > today) {
                            return `<div class="text-center">
                                <a onclick=lockUnlock('${data.id}') class="btn btn-danger text-white">
                                <i class="fas fa-lock-open"></i>&nbsp; Unlock</a>
                            </div>`;
                        }
                        else {
                            return `<div class="text-center">
                                <a onclick=lockUnlock('${data.id}') class="btn btn-success text-white">
                                <i class="fas fa-lock"></i>&nbsp; Lock</a>
                            </div>`;
                        }
                }, "width": "25%"
            }
        ]

    });
}

function lockUnlock(id) {        
                $.ajax({
                    url: "/Admin/User/LockUnlock",
                    type: "POST",
                    data: JSON.stringify(id),
                    contentType:"application/json",
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
       
