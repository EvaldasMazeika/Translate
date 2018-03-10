$(document).ready(function () {

    $('#DeletePostModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget)
        var recipient = button.data('whatever')

        var modal = $(this)

        $("#deletePost").click(function (e) {
            e.preventDefault();
            $.ajax({
                type: "POST",
                url: "/Admin/DeletePost/" + recipient,
                data: {
                    id: recipient,
                },
                success: function (result) {
                    location.reload();
                },
                error: function (result) {
                }
            });
        });
    })

    // accept project invitation
    $("#acceptInv").click(function (e) {
        e.preventDefault();
        var button = $(e.currentTarget);
        var project = button.data('projectid');
        var employee = button.data('me');
        $.ajax({
            type: "POST",
            url: "/Home/AcceptInvAsync",
            data: JSON.stringify({
                Pr: project,
                Emp: employee
            }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                $("#invits").load("Home/Invitations");
                $("#projects").load("Home/ProjectsAsync");
            },
            error: function (response) {

            }
        });
    });

    //decline invitation AJAX
    $("#declineInv").click(function (e) {
        e.preventDefault();
        var button = $(e.currentTarget);
        var project = button.data('projectid');
        var employee = button.data('me');
        $.ajax({
            type: "POST",
            url: "/Home/DeclineInvAsync",
            data: JSON.stringify({
                Pr: project,
                Emp: employee
            }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                $("#invits").load("Home/Invitations");
            },
            error: function (response) {

            }
        });
    });

});
