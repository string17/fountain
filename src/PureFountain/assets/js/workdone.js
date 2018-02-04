
   $(function () {

    $('#picture_file').fileupload({
        url: $("input:hidden[name='upService']").val(),
        dataType: 'json',
        disableImageResize: /Android(?!.*Chrome)|Opera/
            .test(window.navigator && navigator.userAgent),
        previewMaxWidth: 90,
        previewMaxHeight: 90,
        previewCrop: true,
        imageOrientation: true,
        imageMaxWidth: 300,
        imageMaxHeight: 300,
        imageCrop: true, // Force cropped images
        maxFileSize: 500000,
        acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i,
        type: 'POST',
        beforeSend: function (xhr) {
           xhr.setRequestHeader($("meta[name='_csrf_header']").attr("content"),$("meta[name='_csrf']").attr("content"));
             progressIndicator("Uploading");
        },
        done: function (e, data) {
            var result = data.result;
            if (data && result.upload_status==200) {
                $('.profile_picture').prop('src', result.imageURL);
		closeProgressIndicator();
                swal("Upload Success", result.upload_message, "success");
            } else if (data && result.upload_status >200) {
		closeProgressIndicator();
                swal("Upload Failed", result.upload_message, "error");
            } else {
		 closeProgressIndicator();
                swal("Upload Failed", "Upload failed, please try again", "error");
            }
         },
        progressall: function (e, data) {
            var progress = parseInt(data.loaded / data.total * 100, 10);
	    $("#message_pram").html(progress+"%");
        }
    }).prop('disabled', !$.support.fileInput)
        .parent().addClass($.support.fileInput ? undefined : 'disabled');
});

    function moduleNavigation(obj){
    	var url = obj.getAttribute("workdone_page_data");
    	document.location.href=url;
    }
    function sendRequest(action,method,ismultipart){
	var securityToken =(ismultipart ==true)?"?"+(($("input:hidden[id=security_token]").attr("name").trim().length > 2)?$("input:hidden[id=security_token]").attr("name"):"token")+"="+$("input:hidden[id=security_token]").val():"";
    	var $form =$("form[name='workdone_form']");
    	$form.attr("action",action+securityToken);
    	$form.attr("method",method);
    	progressIndicator("Processing");
    	$form.submit();
    }
    function performRequest(action,method,id){
    	$("input:hidden[name='selected_id']").val(id);
    	var $form =$("form[name='workdone_form']");
    	$form.attr("action",action);
    	$form.attr("method",method);
    	progressIndicator("Loading");
    	$form.submit();
    }
    function progressIndicator(indicatormessage){
    	$("#message_dialog").css("display","none");
    	$("#message_pram").html(indicatormessage);
    	$("#message_dialog").css("display","block");	
    }
    function closeProgressIndicator() {
        setTimeout(function () { $("#message_dialog").css("display", "none"); }, 2000);
    	
    }
    