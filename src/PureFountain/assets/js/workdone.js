    (function () {
        //Toggle
        $('body').on('click', '#menu-trigger', function (e) {

            ///alert("contact");
            e.preventDefault();
            var x = $(this).data('trigger');

            $(x).toggleClass('toggled');
            $(this).toggleClass('open');
            $('body').toggleClass('modal-open');

            //Close opened sub-menus
            $('.sub-menu.toggled').not('.active').each(function () {
                $(this).removeClass('toggled');
                $(this).find('ul').hide();
            });



               if (x == '#sidebar') {

                $elem = '#sidebar';
                $elem2 = '#menu-trigger';             
                $('#header').toggleClass('sidebar-toggled');               
           	}

             //When clicking outside
            if ($('#header').hasClass('sidebar-toggled')) {
                $(document).on('click', function (e) {
                    if (($(e.target).closest($elem).length === 0) && ($(e.target).closest($elem2).length === 0)) {
                        setTimeout(function () {
                            $('body').removeClass('modal-open');
                            $($elem).removeClass('toggled');
                            $('#header').removeClass('sidebar-toggled');
                            $($elem2).removeClass('open');
                        });
                    }
                });
            }
        })

        //Submenu
        $('body').on('click', '.sub-menu > a', function (e) {
            e.preventDefault();
            $(this).next().slideToggle(200);
            $(this).parent().toggleClass('toggled');
        });
	if ($('.fg-line')[0]) {
        $('body').on('focus', '.form-control', function () {
            $(this).closest('.fg-line').addClass('fg-toggled');
        })

        $('body').on('blur', '.form-control', function () {
            var p = $(this).closest('.form-group');
            var i = p.find('.form-control').val();

            if (p.hasClass('fg-float')) {
                if (i.length == 0) {
                    $(this).closest('.fg-line').removeClass('fg-toggled');
                }
            }
            else {
                $(this).closest('.fg-line').removeClass('fg-toggled');
            }
        });
    }

    //Add blue animated border and remove with condition when focus and blur
    if ($('.fg-line')[0]) {
        $('body').on('focus', '.form-control', function () {
            $(this).closest('.fg-line').addClass('fg-toggled');
        })

        $('body').on('blur', '.form-control', function () {
            var p = $(this).closest('.form-group');
            var i = p.find('.form-control').val();

            if (p.hasClass('fg-float')) {
                if (i.length == 0) {
                    $(this).closest('.fg-line').removeClass('fg-toggled');
                }
            }
            else {
                $(this).closest('.fg-line').removeClass('fg-toggled');
            }
        });
    }

    //Add blue border for pre-valued fg-flot text feilds
    if ($('.fg-float')[0]) {
        $('.fg-float .form-control').each(function () {
            var i = $(this).val();

            if (!i.length == 0) {
                $(this).closest('.fg-line').addClass('fg-toggled');
            }

        });
    }


    })();
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
    function clearFieldsAndSubmit(fields,action,method){
    	 if(fields.length >0){
    		  for(var i=0; i< fields.length; i++){
			  if(fields[i].type=="hidden"){
    				    $("input:hidden[name='"+fields[i].name+"']").val("");
    			   }else if(fields[i].type=="checkbox"){
    				     $("input:checkbox[name='"+fields[i].name+"']").each(function(index){
    				    	 $(this).prop("checked",false); 
    				     });
    			   }else if(fields[i].type=="radio"){
    				   $("input:radio[name='"+fields[i].name+"']").each(function(index){
  				    	 $(this).prop("checked",false); 
  				     });
    			   }
    		  }
    	 }
    	 sendRequest(action,method,false);
    }
$(document).ready(function(){
	 var action =(document.getElementsByName("script_action").length >0)?document.getElementsByName("script_action")[0].value:"";
	if(action=="dateofbirth"){
	   $("#datetimepicker").each(function(){
		$(this).datetimepicker({
                format: 'YYYY-MM-DD'                
            });
	   });
	}
 var tableView =(document.getElementsByName("tableView_Data").length >0)?document.getElementsByName("tableView_Data")[0].value:"";
if(tableView=="searchableTable"){
 $(".table").bootgrid({
            templates: {
                search: "",
                actionButton: "",
                actionDropDown: "",
                actionDropDownItem: "",
                actionDropDownCheckboxItem: "",
            },
            css: {
                icon: 'zmdi icon',
                iconColumns: 'zmdi-view-module',
                iconDown: 'zmdi-expand-more',
                iconRefresh: 'zmdi-refresh',
                header: '',
                iconUp: 'zmdi-expand-less'
            }

        });
}
try{
	 $('.selectpicker').selectpicker();
}catch(e){}
});

function enableChilds(obj,childs){
	 var status =obj.checked;
	 for(var i=0; i< childs.length; i++){
	    $("#"+childs[i]).prop("disabled",status);
	 }
}

function ViewNavigation(obj){
	$("input:hidden[name='mwz']").val(obj.getAttribute("ms_id"));
	$("input:hidden[name='iwz']").val(obj.getAttribute("mis_id"));
	progressIndicator("LOADING.");
	document.location.href=obj.getAttribute("view_page_data");
	
}
function viewServiceSelectIcon(obj){
	var value = obj.options[obj.selectedIndex].value;
	if(value=="0"){
		$("#service_category-icon").prop("src","img/icon-s.png");
	}else{
	  $("#service_category-icon").prop("src","img/offer_icons/"+value);
       }
}
function checkElement(obj){
	if(obj.value ==obj.getAttribute("placeholder")){
		obj.value="";
	}else if(obj.value=="" && obj.getAttribute("placeholder") !=""){
		obj.value =obj.getAttribute("placeholder");
	}
}