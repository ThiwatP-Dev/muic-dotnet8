let allDoc = $('#DocumentLock');
let allPay = $('#IsPaymentLock');
let allRegist = $('#IsRegistrationLock');
let allVisa = $('#VisaLock');
let allGrad = $('#GraduationLock');
let allChangeFac = $('#ChangeFacAndDepLock');
let allSignIn = $('#IsSignInLock');

let allcheckbox = $('[id*=".Locked"]');

$(document).on('click','.js-delete', function() {
  var incident = $(this).closest('tr').find('.js-incident').text();
  $("#delete-confirm-modal-content").empty().html("Are you sure to delete this <br> Incident : " + incident + " ?");
});

$(document).on('click','.js-unlock', function() {
  var incident = $(this).closest('tr').find('.js-incident').text();
  $("#update-confirm-modal-content").empty().html("Are you sure to unlock this <br> Incident : " + incident + " ?");
});
// $(document).ready(function() {

//   allDoc.prop("disabled", true);
//   allPay.prop("disabled", true);
//   allRegist.prop("disabled", true);
//   allVisa.prop("disabled", true);
//   allGrad.prop("disabled", true);
//   allChangeFac.prop("disabled", true);

//   $('.js-checkbox').change(function() {
//     updateAll();
//   });
  
//   if ($('[id$="LockedDocument"]:checked').length > 0) {
//     allDoc.prop("checked", true);
//     allDoc.trigger("chosen:updated");
//     allDoc.prop("disabled", true);
//   } else {
//     allDoc.prop("checked", false);
//     allDoc.trigger("chosen:updated");
//     allDoc.prop("disabled", false);
//   }
//   if ($('[id$="LockedPayment"]:checked').length > 0) {
//     allPay.prop("checked", true);
//     allPay.trigger("chosen:updated");
//     allPay.prop("disabled", true);
//   } else {
//     allPay.prop("checked", false);
//     allPay.trigger("chosen:updated");
//     allPay.prop("disabled", false);
//   }
//   if ($('[id$="LockedRegistration"]:checked').length > 0) {
//     allRegist.prop("checked", true);
//     allRegist.trigger("chosen:updated");
//     allRegist.prop("disabled", true);
//   } else {
//     allRegist.prop("checked", false);
//     allRegist.trigger("chosen:updated");
//     allRegist.prop("disabled", false);
//   }
//   if ($('[id$="LockedVisa"]:checked').length > 0) {
//     allVisa.prop("checked", true);
//     allVisa.trigger("chosen:updated");
//     allVisa.prop("disabled", true);
//   } else {
//     allVisa.prop("checked", false);
//     allVisa.trigger("chosen:updated");
//     allVisa.prop("disabled", false);
//   }
//   if ($('[id$="LockedGraduation"]:checked').length > 0) {
//     allGrad.prop("checked", true);
//     allGrad.trigger("chosen:updated");
//     allGrad.prop("disabled", true);
//   } else {
//     allGrad.prop("checked", false);
//     allGrad.trigger("chosen:updated");
//     allGrad.prop("disabled", false);
//   }
//   if ($('[id$="LockedChangeFaculty"]:checked').length > 0) {
//     allChangeFac.prop("checked", true);
//     allChangeFac.trigger("chosen:updated");
//     allChangeFac.prop("disabled", true);
//   } else {
//     allChangeFac.prop("checked", false);
//     allChangeFac.trigger("chosen:updated");
//     allChangeFac.prop("disabled", false);
//   }
//   if ($('[id$="LockedSignIn"]:checked').length > 0) {
//     allSignIn.prop("checked", true);
//     allSignIn.trigger("chosen:updated");
//     allSignIn.prop("disabled", true);
//   } else {
//     allSignIn.prop("checked", false);
//     allSignIn.trigger("chosen:updated");
//     allSignIn.prop("disabled", false);
//   }
// });

// allDoc.click(function() {
//   $('[id$="LockedDocument"]').prop("checked",$(this).prop("checked"));
//   $('[id$="LockedDocument"]').trigger("chosen:updated");
// });
// allPay.click(function() {
//   $('[id$="LockedPayment"]').prop("checked",$(this).prop("checked"));
//   $('[id$="LockedPayment"]').trigger("chosen:updated");
// });
// allRegist.click(function() {
//   $('[id$="LockedRegistration"]').prop("checked",$(this).prop("checked"));
//   $('[id$="LockedRegistration"]').trigger("chosen:updated");
// });
// allVisa.click(function() {
//   $('[id$="LockedVisa"]').prop("checked",$(this).prop("checked"));
//   $('[id$="LockedVisa"]').trigger("chosen:updated");
// });
// allGrad.click(function() {
//   $('[id$="LockedGraduation"]').prop("checked",$(this).prop("checked"));
//   $('[id$="LockedGraduation"]').trigger("chosen:updated");
// });
// allChangeFac.click(function() {
//   $('[id$="LockedChangeFaculty"]').prop("checked",$(this).prop("checked"));
//   $('[id$="LockedChangeFaculty"]').trigger("chosen:updated");
// });
// allSignIn.click(function() {
//   $('[id$="LockedSignIn"]').prop("checked",$(this).prop("checked"));
//   $('[id$="LockedSignIn"]').trigger("chosen:updated");
// });

// function updateAll() {

//   if ($('[id$="LockedDocument"]:checked').length > 0) {
//     allDoc.prop("checked", true);
//     allDoc.trigger("chosen:updated");
//     allDoc.prop("disabled", true);
//   } else {
//     allDoc.prop("checked", false);
//     allDoc.trigger("chosen:updated");
//     allDoc.prop("disabled", false);
//   }

//   if ($('[id$="LockedPayment"]:checked').length > 0) {
//     allPay.prop("checked", true);
//     allPay.trigger("chosen:updated");
//     allPay.prop("disabled", true);
//   } else {
//     allPay.prop("checked", false);
//     allPay.trigger("chosen:updated");
//     allPay.prop("disabled", false);
//   }

//   if ($('[id$="LockedRegistration"]:checked').length > 0) {
//     allRegist.prop("checked", true);
//     allRegist.trigger("chosen:updated");
//     allRegist.prop("disabled", true);
//   } else {
//     allRegist.prop("checked", false);
//     allRegist.trigger("chosen:updated");
//     allRegist.prop("disabled", false);
//   }

//   if ($('[id$="LockedVisa"]:checked').length > 0) {
//     allVisa.prop("checked", true);
//     allVisa.trigger("chosen:updated");
//     allVisa.prop("disabled", true);
//   } else {
//     allVisa.prop("checked",false);
//     allVisa.trigger("chosen:updated");
//     allVisa.prop("disabled", false);
//   }

//   if ($('[id$="LockedGraduation"]:checked').length > 0) {
//     allGrad.prop("checked", true);
//     allGrad.trigger("chosen:updated");
//     allGrad.prop("disabled", true);
//   } else {
//     allGrad.prop("checked", false);
//     allGrad.trigger("chosen:updated");
//     allGrad.prop("disabled", false);
//   }

//   if ($('[id$="LockedChangeFaculty"]:checked').length > 0) {
//     allChangeFac.prop("checked", true);
//     allChangeFac.trigger("chosen:updated");
//     allChangeFac.prop("disabled", true);
//   } else {
//     allChangeFac.prop("checked", false);
//     allChangeFac.trigger("chosen:updated");
//     allChangeFac.prop("disabled", false);
//   }

//   if ($('[id$="LockedSignIn"]:checked').length > 0) {
//     allSignIn.prop("checked", true);
//     allSignIn.trigger("chosen:updated");
//     allSignIn.prop("disabled", true);
//   } else {
//     allSignIn.prop("checked", false);
//     allSignIn.trigger("chosen:updated");
//     allSignIn.prop("disabled", false);
//   }
// }