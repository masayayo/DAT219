// Write your Javascript code.

/* All the methods inside the scope will run when the page is finish loading */
$(document).ready(function () 
{
    
    /* Datetimepicker */
    /* https://eonasdan.github.io/bootstrap-datetimepicker/ */
    $('#dateStart1').datetimepicker({
        locale: 'nb',
        format: 'MM-DD-YYYY HH:mm:ss'
    });
    $('#dateEnd1').datetimepicker({
        locale: 'nb',
        format: 'MM-DD-YYYY HH:mm:ss',
        useCurrent: false //Important! See issue #1075
    });
    $('#dateStart2').datetimepicker({
        locale: 'nb',
        format: 'MM-DD-YYYY HH:mm:ss'
    });
    $('#dateEnd2').datetimepicker({
        locale: 'nb',
        format: 'MM-DD-YYYY HH:mm:ss',
        useCurrent: false //Important! See issue #1075 
    });
    // Prevents the registartion end date to come before the registration start date.
    $("#dateStart1").on("dp.change", function (e) {
            $('#dateEnd1').data("DateTimePicker").minDate(e.date);
        });
    // Prevents the registartion start date to come after the registration end date.
    $("#dateEnd1").on("dp.change", function (e) {
        $('#dateStart1').data("DateTimePicker").maxDate(e.date);
    });
    // Prevents the tournament end date to come before the tournament start date.
    $("#dateStart2").on("dp.change", function (e) {
            $('#dateEnd2').data("DateTimePicker").minDate(e.date);
        });
    // Prevents the tournament start date to come after the tournament end date.
    $("#dateEnd2").on("dp.change", function (e) {
        $('#dateStart2').data("DateTimePicker").maxDate(e.date);
    });
    // Prevents the registration end date to come after the tournament start date.
    $("#dateStart2").on("dp.change", function (e) {
            $('#dateEnd1').data("DateTimePicker").maxDate(e.date);
        });
    // Prevents the tournament start date to come before the registration end date.
    $("#dateEnd1").on("dp.change", function (e) {
        $('#dateStart2').data("DateTimePicker").minDate(e.date);
    });

      $('#BirthDay').datetimepicker({
        locale: 'nb',
        format: 'YYYY-MM-DD',
        useCurrent: false //Important! See issue #1075 
    });
    /* Terms accepted? */
    /* http://stackoverflow.com/questions/26122171/how-to-add-boolean-required-attribute-in-mvc */
     // extend jquery range validator to work for required checkboxes
    var defaultRangeValidator = $.validator.methods.range;
    $.validator.methods.range = function(value, element, param) {
        if(element.type === 'checkbox') {
            // if it's a checkbox return true if it is checked
            return element.checked;
        } else {
            // otherwise run the default validation function
            return defaultRangeValidator.call(this, value, element, param);
        }
    }

    /* Age lists */
    /* When a start age have been selected first */
    $('#AgeFrom').on('change', function()
    {    
        /* Show all the option values in the end age value list if the user has changed the selected start age value more than once. */
        $('#AgeTo option').each(function()
        {
            $(this).show();
        })
        
        /* Get the start age value. */
        var $SelectedAgeFrom = parseInt($('#AgeFrom').val());
        /* Iterate through the end age values and hide the ones who is less than the chosen start age value. */
        $('#AgeTo option').each(function()
        {
            if(parseInt($(this).val()) < $SelectedAgeFrom)
            {
                $(this).hide();
            }
            /* If the current age value is equal to the new start age value */
            else if(parseInt($(this).val()) == $SelectedAgeFrom)
            {
                /* Then we only change the current selected end age value if it is less than the recently choosen start age value.
                   In that case we change it to the value of the latter. */
                var currentAgeToSelected = parseInt($('#AgeTo').val());
                if(currentAgeToSelected < $SelectedAgeFrom)
                {
                    $('#AgeTo').val($SelectedAgeFrom);
                }     
            }
        })
    })

    /* AJAX CALLS */
    /* Sort Tournamentlist */

    /* Someone has clicked an anchor element in the table. */
    $("a.TournamentListColumnName").click(function()
    {
        /* Get the name of the table column and the sorting order */
        var sortname = $(this).attr("name");
        var decreasingorder = $(this).attr("value");

        /* If the sorting order is decreasing - set it to increasing*/
        if(decreasingorder === "true")
        {
            $(this).attr("value",false);
            /* Finds the first span element with class=arrow after the clicked a element. Replaces the fontawesome arrow with a opposite. */
            $(this).nextAll(".arrow").first().replaceWith('<span class="arrow">&nbsp; &nbsp;<i class="fa fa-angle-down fa-2x" aria-hidden="true"></i></span>');
        }
        else
        {
            $(this).attr("value",true);
            $(this).nextAll(".arrow").first().replaceWith('<span class="arrow">&nbsp; &nbsp;<i class="fa fa-angle-up fa-2x" aria-hidden="true"></i></span>');
        }
        /* A call to the ajax method */
        SortTournamentTable(sortname, decreasingorder);
    });
    /* This is the ajax request to the server. The serverside 
    returns the tabledata sorted after sortname and in the correct order. */
    function SortTournamentTable(sortname, decreasingorder)
    {
        $.ajax
            ({
                type: "GET",
                url: "/Tournament/SortTournamentList",
                data: {sortname: sortname, decreasingorder: decreasingorder},
                beforeSend: function()
                {
                    /* Add a spinner */
                    $("#TournamentTableContent").append('<i id="spinner" style=" position:fixed ; z-index: 999; margin-left: 490px; margin-top:-250px" class="fa fa-spinner fa-pulse fa-3x fa-fw"></i>');
                },
                complete: function()
                {   
                    /* Remove the spinner */
                    $("#spinner").remove();
                },
                success: function(respons)
                {   
                    /* Dumps the html content into the table body */
                    $("#TournamentRespons").html(respons);
                },
                failure: function()
                {
                    $("#TournamentTableContent").prepend('<div class="alert alert-warning">Noe gikk galt, prøv igjen senere!</div>');
                }
            });
    };


    /* Controlls related to the differnt registrations */
   
    /**************************************/
    /** Individual registration **/
    /**************************************/
    
    /* Form is getting submitted */
    $("#IndividualRegForm").submit(function(e)
    {
        /* Prevents the form from being submitted */
        e.preventDefault();
        /* We notify user if the radiobox is not checked */
        if(!($("#IndividualRadioBox").is(":checked")) && $("#ErrorMessageRadioBoxIsNotChecked").length == 0)
        {
            $("#IndividualRadioBox").before('<div class="bg-danger" id="ErrorMessageRadioBoxIsNotChecked">Du må sjekke av boksen!</div>');
        }
        else if(($("#IndividualRadioBox").is(":checked"))) /* If the radiobox is checked */
        {
            /* Use ajax to do different user controls. We do a post if Ajax success. Else we give a informativ errormessage.  */
            $.ajax
            ({
                type: "GET",
                url: "/Registration/IndividualRegistrationCheck",
                data: {tournamentId: $("#TournamentId").val() , applicationId: $("#IndividualUserId").val()},
                beforeSend: function()
                {
                    /* Add a spinner */
                    $("#IndividualRadioBox").after('<i id="spinner" style=" position:fixed ; z-index: 999; margin-left: 100px; margin-top:-50px" class="fa fa-spinner fa-pulse fa-3x fa-fw"></i>');      
                },
                complete: function()
                {   
                    /* Remove spinner */
                    $("#spinner").remove();
                },
                success: function(respons) 
                {   
                    /* If failure */
                    if(respons != "")
                    {
                        if($("#ErrorMessageAllreadyReg").length != 0)
                        {
                            $("#ErrorMessageAllreadyReg").remove();
                        }
                        $("#TheTeamContainerDiv").before("<div class='bg-danger' id='ErrorMessageAllreadyReg'>" +respons +"</div>");
                    }
                    /* If success */
                    else if(respons == "") /* We send the form data to the server. We trigger the submit event directly (this will not trigger any event bind on the submit event before the actually submit of the form). This will redirect us to the post request. */
                    {
                        $("#IndividualRegForm").get(0).submit(); 
                    }
                },
                failure: function()
                {
                    /* Remove spinner */
                    $("#spinner").remove();
                    $("#TheTeamContainerDiv").before('<div class="bg-danger" id="ErrorMessageAllreadyReg"> Noe gikk galt. Prøv igjen!</div>');      
                }
            });
        }
    });

    /* We remove the errormessage related to unchecked radiobox if the
    radiobox gets checked */
    $("#IndividualRadioBox").on("change", function()
    {
        if($("#IndividualRadioBox").is(":checked"))
        {
            $("#ErrorMessageRadioBoxIsNotChecked").remove();
        }
    });

    /**************************************/
    /** Team registration **/
    /**************************************/

    /* Form is getting submitted */
    $("#TeamRegForm").submit(function(e)
    {
        /* Prevent a form submit */
        e.preventDefault();
        /* Check to see that the radiobox is checked and if not that the errormessage only is given once */
        if(!$("#TeamRadioBox").is(":checked") && $("#ErrorMessageRadioBoxIsNotChecked").length == 0)
        {
            $("#TeamRadioBox").before('<div class="bg-danger" id="ErrorMessageRadioBoxIsNotChecked">Du må sjekke av boksen!</div>');
        }
        /* Check to see if a team has been selected and if not that a errormessage only is given once */
        if($("#SelectedTeam").children("option:first").text() == "" && $("#ErrorMessageTeamIsNotChecked").length == 0)
        {
            $("#TeamRadioBox").before('<div class="bg-danger" id="ErrorMessageTeamIsNotChecked">Du må velge et lag fra listen! Hvis du ikke er kontaktperson for noen lag må du først gå tilbake å lage et!</div>');
        }
        /* If radiobox is checked and a team is selected */
        if($("#TeamRadioBox").is(":checked") && $("#SelectedTeam").children("option:first").text()!="")
        {
            /* Use ajax to do different user controls. We do a post if Ajax success. Else we give a informativ errormessage. */
            $.ajax
            ({
                type: "GET",
                url: "/Registration/TeamRegistrationCheck",
                /* Data consists of the tournamentid and teamid */
                data: {tournamentId: $("#TournamentId").val() , teamId: $("#SelectedTeam").children("option:first").val()},
                //timeout: 2000,
                beforeSend: function()
                {
                    /* Add a spinner */
                    $("#TeamRadioBox").after('<i id="spinner" style=" position:fixed ; z-index: 999; margin-left: 100px; margin-top:-50px" class="fa fa-spinner fa-pulse fa-3x fa-fw"></i>');      
                },
                complete: function()
                {   
                    /* Remove spinner */
                    $("#spinner").remove();
                },
                success: function(respons) 
                {   
                    /* If failure */ 
                    if(respons != "")
                    {
                        if($("#ErrorMessageAllreadyReg").length != 0)
                        {
                            $("#ErrorMessageAllreadyReg").remove();
                        }
                        $("#TheTeamContainerDiv").before("<div class='bg-danger' id='ErrorMessageAllreadyReg'>" +respons +"</div>");
                    }
                    /* If success */
                    else if(respons == "") /* We send the form data to the server. We trigger the submit event directly (this will not trigger any event bind on the submit event before the actually submit of the form). This will redirect us to the post request on the serverside. */
                    {
                        $("#TeamRegForm").attr("action", "/Registration/TeamRegistration/4?tournamentId=" + $("#TournamentId").val() + "&teamId="+$("#SelectedTeam").children("option:first").val() +"&applicationId=" + $("#IndividualUserId").val());
                        $("#TeamRegForm").get(0).submit(); 
                    }
                },
                failure: function()
                {
                    /* Remove spinner */
                    $("#spinner").remove();
                    $("#TeamRadioBox").before('<div class="bg-danger" id="ErrorMessageAllreadyReg"> Noe gikk galt. Prøv igjen!</div>');
                }
            });
        }
    });

    /* We remove the errormessages related to unchecked radiobox if the
    radiobox gets checked */
    $("#TeamRadioBox").on("change", function()
    {
        if($("#TeamRadioBox").is(":checked"))
        {
            $("#ErrorMessageRadioBoxIsNotChecked").remove();
        }
    });
    /* We add a eventlistener on the team selectlist */
    $("#TeamList").on("change", function()
    {
        /* Get the text and value from the selected option and set it into the other selectlist which consists of only one option */
        var text = $("#TeamList option:selected").text();
        var value = $("#TeamList option:selected").val();
        $("#SelectedTeam").children("option:first").text(text);
        $("#SelectedTeam").children("option:first").val(value);
        /* Renove error message related to unselected team */
        $("#ErrorMessageTeamIsNotChecked").remove();
    });

    /**************************************/
    /** Individual or Team registration  **/
    /**************************************/
    
    /* Used to customize the registration page after the user registartion choice */
    var DetachedTeamDiv = $("#ChooseTeam");
    var AttachedElement = $("#TheTeamContainerDiv");

    /* If non of the checkboxes is selected we selct the first one and detach team elements */
    if( !$("#MultiIndividualRadioBox").is(":checked") && !$("#MultiTeamRadioBox").is(":checked") )
    {
        $("#MultiIndividualRadioBox").attr("checked", true);
        DetachedTeamDiv = $("#ChooseTeam").detach();
    }

    /* Eventlistner on individual registration radiobox */
    $("#MultiIndividualRadioBox").on("change", function()
    {
        /* We remove the team elements if the individual radiobox is selected */
        if($("#MultiIndividualRadioBox").is(":checked"))
        {
            DetachedTeamDiv = $("#ChooseTeam").detach();
        }
        /* If there is a error message hanging around after the team radio box has been selected */
        if($("#ErrorMessageAllreadyReg").length != 0)
        {
            $("#ErrorMessageAllreadyReg").remove();
        }
    });
    /* Eventlistner on team registration radiobox */
    $("#MultiTeamRadioBox").on("change", function()
    {
        /* We add the team elements if the team radiobox is selected */
        if($("#MultiTeamRadioBox").is(":checked"))
        {
            $("#ErrorMessageAllreadyReg").remove();
            DetachedTeamDiv.appendTo(AttachedElement);
        }
    });

    /* The individual form is getting submitted */
    $("#MultiForm").submit(function(e)
    {
        /* If the user has choosed a individual registration */
        if($("#MultiIndividualRadioBox").is(":checked"))
        {
            /* Prevents the form from being submitted */
            e.preventDefault();
            /* We notify user if the radiobox is not checked */
            if(!($("#MultiIndividualRadioBox").is(":checked")) && $("#ErrorMessageRadioBoxIsNotChecked").length == 0)
            {
                $("#MultiIndividualRadioBox").before('<div class="bg-danger" id="ErrorMessageRadioBoxIsNotChecked">Du må sjekke av boksen!</div>');
            }
            else if(($("#MultiIndividualRadioBox").is(":checked"))) /* If the radiobox is checked */
            {
                /* Use ajax to do different user controls. We do a post if Ajax success. Else we give a informativ errormessage. */
                $.ajax
                ({
                    type: "GET",
                    url: "/Registration/IndividualRegistrationCheck",
                    data: {tournamentId: $("#TournamentId").val() , applicationId: $("#IndividualUserId").val()},
                    beforeSend: function()
                    {
                        /* Add a spinner */
                        $("#MultiIndividualRadioBox").after('<i id="spinner" style=" position:fixed ; z-index: 999; margin-left: 100px; margin-top:-50px" class="fa fa-spinner fa-pulse fa-3x fa-fw"></i>');      
                    },
                    complete: function()
                    {   
                        /* Remove spinner */
                        $("#spinner").remove();
                    },
                    success: function(respons) 
                    {   
                        /* We do not allow the user to be register again if he has been registered before */
                        if(respons != "")
                        {
                            if($("#ErrorMessageAllreadyReg").length != 0)
                            {
                                $("#ErrorMessageAllreadyReg").remove();
                            }
                            $("#TheTeamContainerDiv").before("<div class='bg-danger' id='ErrorMessageAllreadyReg'>" +respons +"</div>");
                        }
                        else if(respons == "") /* We send the form data to the server. We trigger the submit event directly (this will not trigger any event bind on the submit event before the actually submit of the form). This will redirect us to the post request. */
                        {
                            $("#MultiForm").get(0).setAttribute("action","/Registration/IndividualRegistration/?applicationId=" + $("#IndividualUserId").val() +"&tournamentId=" + $("#TournamentId").val() ); 
                            $("#MultiForm").get(0).submit(); 
                        }
                    },
                    failure: function()
                    {
                        /* Remove spinner */
                        $("#spinner").remove();
                        $("#TheTeamContainerDiv").before('<div class="bg-danger" id="ErrorMessageAllreadyReg"> Noe gikk galt. Prøv igjen!</div>');   
                    }
                });
            }
        }
        /* The user has choosen a team registration */
        else if($("#MultiTeamRadioBox").is(":checked"))
        {
            /* Prevent a form submit */
            e.preventDefault();
            /* Check to see that the radiobox is checked and if not that the errormessage only is given once */
            if(!$("#MultiTeamRadioBox").is(":checked") && $("#ErrorMessageRadioBoxIsNotChecked").length == 0)
            {
                $("#MultiTeamRadioBox").before('<div class="bg-danger" id="ErrorMessageRadioBoxIsNotChecked">Du må sjekke av boksen!</div>');
            }
            /* Check to see if a team has been selected and if not that a errormessage only is given once */
            if($("#SelectedTeam").children("option:first").text() == "" && $("#ErrorMessageTeamIsNotChecked").length == 0)
            {
                $("#TeamList").before('<div class="bg-danger" id="ErrorMessageTeamIsNotChecked">Du må velge et lag fra listen! Hvis du ikke er kontaktperson for noen lag må du først gå tilbake å lage et!</div>');
            }
            /* If radiobox is checked and a team is selected */
            if($("#MultiTeamRadioBox").is(":checked") && $("#SelectedTeam").children("option:first").text()!="")
            {
                /* Use ajax to do different user controls. We do a post if Ajax success. Else we give a informativ errormessage. */
                $.ajax
                ({
                    type: "GET",
                    url: "/Registration/IndividualOrTeamRegistrationCheck",
                    /* Data consists of the tournamentid and teamid */
                    data: {tournamentId: $("#TournamentId").val() , teamId: $("#SelectedTeam").children("option:first").val()},
                    //timeout: 2000,
                    beforeSend: function()
                    {
                        /* Add a spinner */
                       $("#MultiTeamRadioBox").after('<i id="spinner" style=" position:fixed ; z-index: 999; margin-left: 100px; margin-top:-50px" class="fa fa-spinner fa-pulse fa-3x fa-fw"></i>');      
                    },
                    complete: function()
                    {   
                       /* Remove spinner */
                       $("#spinner").remove();
                    },
                    success: function(respons) 
                    {   
                        /* If failure */
                        if(respons != "")
                        {
                            if($("#ErrorMessageAllreadyReg").length != 0)
                            {
                                $("#ErrorMessageAllreadyReg").remove();
                            }
                            $("#TheTeamContainerDiv").before("<div class='bg-danger' id='ErrorMessageAllreadyReg'>" +respons +"</div>");
                        }
                        /* If success */
                        else if(respons == "") /* We send the form data to the server. We trigger the submit event directly (this will not trigger any event bind on the submit event before the actually submit of the form). This will redirect us to the post request on the serverside. */
                        {
                            $("#MultiForm").get(0).setAttribute("action", "/Registration/TeamRegistration/?tournamentId=" + $("#TournamentId").val() + "&teamId="+$("#SelectedTeam").children("option:first").val() +"&applicationId=" + $("#IndividualUserId").val()); 
                            $("#MultiForm").get(0).submit(); 
                        }
                    },
                    failure: function()
                    {
                        /* Remove spinner */
                        $("#spinner").remove();
                        $("#TheTeamContainerDiv").before('<div class="bg-danger" id="ErrorMessageAllreadyReg"> Noe gikk galt. Prøv igjen!</div>');
                    }
                });
            }
        }
    });  
});
