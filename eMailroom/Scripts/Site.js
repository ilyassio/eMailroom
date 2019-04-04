var MailUploadMsg;
var AttachementsUploadMsg;
var EmptyRequiredField;
var PasswordConfirmationError;
var SelectMoreThanOneFileError;
var AddNewContact;
var ChooseSelect;

$(document).ready(function (e) {
    function refresh() {
        $('#drop-text-mail').text(MailUploadMsg);
        $('#drop-text-attachements').text(AttachementsUploadMsg);
        $('form').each(function () { this.reset() });
        $("#main-alert").removeClass();
        $("#main-alert").html('');
        $('#select-sender').selectpicker('refresh');
        $('#select-receiver').selectpicker('refresh');
        $('#select-type').selectpicker('refresh');
    }
    function refreshInputsChangePassword() {
        $('#input-confirm-password').removeClass('is-invalid');
        $('#input-confirm-password').removeClass('is-valid');
        $('#btn-change-password').addClass('d-none');
    }
    $('#form-change-password').submit(function (e) {
        e.preventDefault();

        function clearAlert() {
            $("#alert-change-password").removeClass();
            $("#alert-change-password").addClass("alert");
        }
        function clearAll() {
            clearAlert();
            $('#inputs-change-password').find("input").val("");
        }
        if ($("#input-old-password").val() == "" || $("#input-new-password").val() == "" || $("#input-confirm-password").val() == "") {

            clearAlert();
            
            $("#alert-change-password").addClass("alert-danger");
            $("#alert-change-password").html(EmptyRequiredField);
            $("#modal-settings").effect("shake");
        }
        else if ($("#input-new-password").val() != $("#input-confirm-password").val()) {

            clearAll();

            $("#alert-change-password").addClass("alert-danger");
            $("#alert-change-password").html(PasswordConfirmationError);
            $("#modal-settings").effect("shake");
        }
        else {
            var formData = new FormData(this);
            $.ajax({
                url: '/Home/ChangePassword',
                type: 'POST',
                data: formData,
                cache: false,
                success: function (response) {
                    if (!response.signedIn)
                        window.location.reload(true);
                    else {
                        clearAll();
                        refreshInputsChangePassword();
                        $("#alert-change-password").addClass("alert-" + response.alertClass);
                        $("#alert-change-password").html(response.alertMessage);

                        if (response.alertClass == "danger")
                            $("#modal-settings").effect("shake");
                    }                    
                },
                contentType: false,
                processData: false
            });
        }
    });

    $('#input-mail').change(function () {

        if (this.files.length == 1) {

            var file = $(this).prop("files");
            var filename = $.map(file, function (val) { return val.name; });

            $('#drop-text-mail').text(filename.join(', '));

            $('#btn-ocr').removeClass('d-none');
        }
        else if (this.files.length == 0) {
            $('#btn-ocr').addClass('d-none');
            $('#drop-text-mail').text(MailUploadMsg);
        }
        else {
            $('#btn-ocr').addClass('d-none');
            $('#drop-text-mail').text(SelectMoreThanOneFileError);
        }
    });

    $('#input-attachements').change(function () {

        if (this.files.length > 0) {
            
            var files = $(this).prop("files");
            var filenames = $.map(files, function (val) { return val.name; });

            $('#drop-text-attachements').text(filenames.join(' ; '));
        }
        else {
            $(this).next().text(AttachementsUploadMsg);
        }
    });

    $("#form-new-mail").submit(function (e) {

        e.preventDefault();
        var formData = new FormData(this);
        refresh();

        $.ajax({
            url: 'AddMail',
            type: 'POST',
            data: formData,
            cache: false,
            success: function (response) {
                
                if (!response.signedIn) {
                    window.location.reload(true);
                }
                else {
                    $("#main-alert").addClass("alert");
                    $('#form-new-mail').find("input").val("");
                    $("#main-alert").addClass("alert-" + response.alertClass);
                    if (response.alertClass == "danger") {
                        $("#main-alert").html('<i class="fas fa-times"></i> ' + response.alertMessage);
                        $("#main-alert").effect("shake");
                    }
                    else if (response.alertClass == "warning") {
                        $("#main-alert").html('<i class="fas fa-exclamation"></i> ' + response.alertMessage);
                        $("#main-alert").effect("shake");
                    }
                    else
                        $("#main-alert").html('<i class="fas fa-check"></i> ' + response.alertMessage);
                }
            },
            contentType: false,
            processData: false
        });
    });

    $('#select-type').on('change', function () {

        $('#select-sender').selectpicker('destroy');
        $('#select-receiver').selectpicker('destroy');
        $('#select-sender').find('option').remove().end();
        $('#select-receiver').find('option').remove().end();
        $('#btn-add-new-contact').remove();

        $.post(
            'GetParties',
            function (response) {
                var Parties = JSON.parse(response);

                var btnAddNewContact = document.createElement("button");
                btnAddNewContact.innerText = AddNewContact;
                btnAddNewContact.setAttribute("type", "button");
                btnAddNewContact.setAttribute("id", "btn-add-new-contact");
                btnAddNewContact.setAttribute("class", "btn btn-block btn-light rounded-0 border-0");

                $("#select-sender").append('<option selected value="" >'+ChooseSelect+'</option>');
                $("#select-receiver").append('<option selected value="" >'+ChooseSelect+'</option>');


                if ($("#select-type").val() == "incoming") {

                    for (var i = 0; Parties.Employees[i]; i++) {
                        $("#select-receiver").append(new Option(Parties.Employees[i].Position + "    " + Parties.Employees[i].Firstname + "    " + Parties.Employees[i].Lastname, Parties.Employees[i].Id));
                    }

                    for (var i = 0; Parties.Contacts[i]; i++) {
                        $("#select-sender").append(new Option(Parties.Contacts[i].Firstname + "    " + Parties.Contacts[i].Lastname + "    " + Parties.Contacts[i].SocialReason, Parties.Contacts[i].Id));
                    }

                    $('#select-sender').selectpicker('refresh');
                    $('#select-receiver').selectpicker('refresh');
                    $('.dropdown-menu')[2].append(btnAddNewContact)

                }
                else if ($("#select-type").val() == "outgoing") {
                    for (var i = 0; Parties.Employees[i]; i++) {
                        $("#select-sender").append(new Option(Parties.Employees[i].Position + "    " + Parties.Employees[i].Firstname + "    " + Parties.Employees[i].Lastname, Parties.Employees[i].Id));
                    }

                    for (var i = 0; Parties.Contacts[i]; i++) {
                        $("#select-receiver").append(new Option(Parties.Contacts[i].Firstname + "    " + Parties.Contacts[i].Lastname + "    " + Parties.Contacts[i].SocialReason, Parties.Contacts[i].Id));
                    }
                    $('#select-sender').selectpicker('refresh');
                    $('#select-receiver').selectpicker('refresh');
                    $('.dropdown-menu')[4].append(btnAddNewContact)
                }
                $('.dropdown-menu').addClass("pb-0");
            }

        );


    });

    $('.drop-area').bind('dragover', function () {
        $(this).addClass('drag-over');
    });

    $('.drop-area').bind('dragleave', function () {
        $(this).removeClass('drag-over');
    });

    $('.drop-area').on('drop', function () {
        $(this).removeClass('drag-over');

    });

    $('#btn-ocr').on('click', function () {
        if ($('#input-mail')[0].files[0].value) {
            alert(document.getElementById("input-mail").files[0].value)
        }
        else {
            var fd = new FormData();
            fd.append("mail", document.getElementById("input-mail").files[0]);
            $.ajax({
                url: 'ApplyOcr',
                type: 'POST',
                data: fd,
                cache: false,
                success: function (response) {
                    $('#mail-upload').hide();
                    $('#attachements-upload').hide();
                    $('#div-ocr-view').addClass('mt-5');
                    $('#div-ocr-view').addClass('p-5');
                    $('#div-ocr-view').show();
                    $('#div-ocr-view').html(response);
                },
                contentType: false,
                processData: false
            });    
        }
    });

    $('#inputs-change-password input').on('keyup', function () {

        refreshInputsChangePassword();

        if ($('#input-confirm-password').val() != "" && $('#input-new-password').val() != "") {
            if ($('#input-confirm-password').val() != $('#input-new-password').val()) {
                $('#input-confirm-password').addClass('is-invalid');
            }
            else {
                $('#input-confirm-password').addClass('is-valid');
                $('#btn-change-password').removeClass('d-none');
            }
        }
       
    });

    $(".btn-edit").on('click', function () {
        $(this).parents('div').prev().removeAttr('readonly');
    });
});