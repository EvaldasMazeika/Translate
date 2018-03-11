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

    //INVITE TO PROJECT AJAX CALL
    $("#addToProject").click(function (e) {
        e.preventDefault();
        var projectId = $("#projectid").val();
        var email = $("#elpastas").val();

        $.ajax({
            type: "POST",
            url: "/Project/" + projectId + "/AddToProjectAsync",
            data: JSON.stringify({
                ProjectId: projectId,
                Email: email
            }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                    $('#AddmemberModal').modal('hide');
                    $("#alerti").append(`<div class="alert alert-success alert-dismissible fade show" role="alert">
  Pakvietimas sėkmingai išsiųstas
  <button type="button" class="close" data-dismiss="alert" aria-label="Close">
    <span aria-hidden="true">&times;</span>
  </button>
</div>`);
            },
            error: function (response) {
                $('#AddmemberModal').modal('hide');
                $("#alerti").append(`<div class="alert alert-danger alert-dismissible fade show" role="alert">
  Įvyko klaida
  <button type="button" class="close" data-dismiss="alert" aria-label="Close">
    <span aria-hidden="true">&times;</span>
  </button>
</div>`);
            }
        });
    });



    //DELETE MEMBER FROM PROJECT AJAX CALL
    $('#DeleteMemberModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget)
        var memberName = button.data('name')
        var memberId = button.data('whatever')
        var projectId = button.data('project')
        var modal = $(this)
        modal.find('.modal-body').text('Ar tikrai norite pašalinti ' + memberName + ' iš projekto?')
        modal.find('#deleteMember').data("whatever", memberId)
        modal.find('#deleteMember').data("project", projectId)
    })

    $("#deleteMember").click(function (e) {
        e.preventDefault();
        var button = $('#deleteMember')
        var memberId = button.data('whatever')
        var projectId = button.data('project')

        $.ajax({
            type: "POST",
            url: "/Project/" + projectId + "/RemoveFromProjectAsync",
            data: JSON.stringify({
                ProjectId: projectId,
                MemberId: memberId
            }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (result) {
                $('#DeleteMemberModal').modal('hide');
                window.setTimeout(function () {
                    $("#teamMembers").load("TeamMembers");  
                }, 1000);
                
            },
            error: function (result) {
                $('#DeleteMemberModal').modal('hide');
                $("#alerti").append(`<div class="alert alert-danger alert-dismissible fade show" role="alert">
  Įvyko klaida
  <button type="button" class="close" data-dismiss="alert" aria-label="Close">
    <span aria-hidden="true">&times;</span>
  </button>
</div>`);
            }
        });
    });

});
