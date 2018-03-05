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
});
