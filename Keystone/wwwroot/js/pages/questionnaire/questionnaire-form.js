var QuestionType = {
    "Ranking": 'r',
    "OneChoice": 'o',
    "MultipleChoice": 'm',
    "ShortAnswer": 's'
};

var arrangeInputIndex = function(obj, index) {
    $($(obj).find(".title :input")).each(function () {
        if ($(this).attr("name")) {
            $(this).attr("name", $(this).attr("name").replace(/\d+/g, index));
        }
    });
};

var arrageAnswerIndexCallback = function(questions) {

    questions.map(function(index, item) {
        var questionId = $(item).attr('id');
        var questionNumber = $(item).find('.question-order .number').html();
        var playground = $(item).find('.js-answer-playground');

        $(playground).attr('id', `${ questionId }-answer`);

        $(`#${ questionId }-answer`).find('.answer').each(function (i, item) {
            $(this).attr('id', `${ questionId }-answer-${ i }`);
            
            $($(item).find(":input")).each(function (j) {
                if ($(this).attr("name")) {
                    $(this).attr("name", $(this).attr("name").replace(/\d+(?=\.[^.]+$)/, j));
                }
            });

            if ($(this).is('select')) {
                $(this).data('target', `#${ questionId }-answer`);
            }
        });

        $(`#${ questionId }-answer`).find('.js-add-option').each(function() {
            $(this).data('target', `#${ questionId }-answer`);
            $(this).data('question-number', questionNumber);
        });
    });
}

var NewQuestion = function() {

    var questionNumber = $("#js-question-body > .question-wrapper").length + 1;
    var newQuestionId = `question-${ questionNumber }`;

    var template = `<div id="${ newQuestionId }" class="question-wrapper">
                        <h4 class="question-order">Question <span class="number">${ questionNumber }</span></h4>
                        <div class="question-body">

                            ${ getQuestionBody(questionNumber, newQuestionId) }

                            <div class="question-footer">
                                <div class="ml-auto" role="group">
                                    <button type="button" class="btn btn--primary next-question-btn js-next-question" data-parent="#${ newQuestionId }">+ Next Question</button>
                                    <button type="button" class="btn btn--danger js-delete-question" data-target="#${ newQuestionId }">Delete</button>
                                </div>
                            </div>
                        </div>
                    </div>`;

    return [template, newQuestionId];
};


var getQuestionBody = function(questionNumber, newQuestionId) {
    return ` <div class="title">
                <div class="form-group row mb-0">
                    <div class="col">
                        <div class="form-control-label d-flex align-items-center">English Question Name</div>
                        <div class="form-controler-input">
                            <input name="QuestionGroup.Questions[0].QuestionEn" type="text" class="form-control" />
                        </div>
                    </div>
                    <div class="col">
                        <div class="form-group">
                            <div class="form-control-label d-flex align-items-center">Thai Question Name</div>
                            <div class="form-controler-input">
                                <input name="QuestionGroup.Questions[${ questionNumber }].QuestionTh" type="text" class="form-control" />
                            </div>
                        </div>
                    </div>
                </div>
                <div class="form-group row mb-0">
                    <div class="col-6">
                        <div class="form-control-label d-flex align-items-center">Question Type</div>
                        <div class="form-control-plaintext">
                            Multiple Choice
                        </div>
                        <div class="form-controler-input">
                            <select role="QuestionType" name="QuestionGroup.Questions[${ questionNumber }].QuestionType" id="QuestionGroup_Questions[${ questionNumber - 1 }]_QuestionType" data-value="" class="form-control js-select-question-type" data-target="#${ newQuestionId }-answer" data-question-number="${ questionNumber }">
                                <option selected disabled value="">Select</option>
                                <option value="r">Ranking</option>
                                <option value="o">One Choice</option>
                                <option value="m">Multiple Choice</option>
                                <option value="s">Short Answer</option>
                            </select>
                        </div>
                    </div>
                    <div class="col-6 d-flex">
                        <div class="form-controler-input align-self-end">
                            <div class="form-check">
                                <input class="form-check-input" type="checkbox" name="QuestionGroup.Questions[${ questionNumber }].IsActive" id="QuestionGroup_Questions[${ questionNumber - 1 }]_IsActive">
                                <label for="QuestionGroup_Questions[${ questionNumber - 1 }]_IsActive">Active</label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div id="${ newQuestionId }-answer" class="js-answer-playground">
                <div class="form-control-label d-flex align-items-center my-4">Answers</div>
                <div class="answer-wrapper"></div>
                
                <a role="button" class="btn btn--primary btn-add-option js-add-option d-none" data-type="" data-target="#${ newQuestionId }-answer" data-question-number="${ questionNumber }">+ add option</a>
            </div>`;
};

$('.questionnaire-body').on('click', '.js-next-question', function(e){

    var question = NewQuestion();
    var template = question[0];
    var newQuestionId = question[1];

    $("#js-question-body").append(template);

    var questions = $("#js-question-body > .question-wrapper");
    questions.map(function(index, item) {
        $(item).removeClass('active');
        arrangeInputIndex(item, index);
    });

    $(`#${ newQuestionId }`).addClass('active');

    $("html, body").animate({ scrollTop: $(document).height()-$(window).height() });
});

$('.questionnaire-body').on('click', '.js-delete-question', function(e) {
    
    if($("#js-question-body > .question-wrapper").length == 1) {
        
        Alert.renderAlert('Unable to delete question', 'Questionnaire is required at least one question', 'warning');

    } else {

        var targetQuestionId = $(e.target).data('target');
        $(targetQuestionId).remove();
        
        var questions = $("#js-question-body > .question-wrapper");

        questions.map(function(index, item) {
            $(item).attr('id', `question-${ index + 1 }`);
            $(item).find('.question-order .number').html(index + 1);
            $(item).find('.question-footer .js-delete-question').data('target', `#question-${ index + 1 }`);

            if(index === questions.length - 1) {
                $(item).addClass('active');
            }

            arrangeInputIndex(item, index);
        });

        return arrageAnswerIndexCallback(questions);
    }
});

var RankingTemplate = function(targetQuestionAnswer, questionNumber, answers) {

    var answerIndex = getAnswerIndex(answers);
    
    return `<div id="question-${ questionNumber }-answer-${ answerIndex }" class="answer col-12 pl-0">
                <div class="form-controler-input d-flex justify-conent-between">
                    <div class="form-check px-0 w-40x">
                        <h5>English Answer</h5>
                        <input data-type="r" class="form-check-input" type="checkbox" id="checkbox1" disabled>
                        <label for="checkbox1"><input name="QuestionGroup.Questions[${ questionNumber - 1 }].Answers[${ answerIndex }].AnswerEn" type="text" class="form-control" /></label>
                    </div>
                    <div class="form-check px-0 w-40x">
                        <h5>Thai Answer</h5>
                        <input data-type="r" class="form-check-input" type="checkbox" id="checkbox1" disabled>
                        <label for="checkbox1"><input name="QuestionGroup.Questions[${ questionNumber - 1 }].Answers[${ answerIndex }].AnswerTh" type="text" class="form-control" /></label>
                    </div>
                    <div class="form-check px-0 w-20x">
                        <h5>Value</h5>
                        <input data-type="r" class="form-check-input" type="checkbox" id="checkbox1" disabled>
                        <label for="checkbox1"><input name="QuestionGroup.Questions[${ questionNumber - 1 }].Answers[${ answerIndex }].Value" type="text" class="form-control" /></label>
                    </div>
                    ${ getRemoveButton(`${ targetQuestionAnswer }-${ answerIndex }`, targetQuestionAnswer) }
                </div>
            </div>`;
};

var OneChoiceTemplate = function(targetQuestionAnswer, questionNumber, answers) {

    var answerIndex = getAnswerIndex(answers);
    
    return `<div id="question-${ questionNumber }-answer-${ answerIndex }" class="answer col-12 pl-0">
                <div class="form-controler-input d-flex justify-conent-between">
                    <div class="form-check px-0 w-40x">
                        <h5>English Answer</h5>
                        <input data-type="m" class="form-check-input" type="radio" id="radio1" disabled>
                        <label for="radio1"><input name="QuestionGroup.Questions[${ questionNumber - 1 }].Answers[${ answerIndex }].AnswerEn" type="text" class="form-control" /></label>
                    </div>
                    <div class="form-check px-0 w-40x">
                        <h5>Thai Answer</h5>
                        <input data-type="m" class="form-check-input" type="radio" id="radio1" disabled>
                        <label for="radio1"><input name="QuestionGroup.Questions[${ questionNumber - 1 }].Answers[${ answerIndex }].AnswerTh" type="text" class="form-control" /></label>
                    </div>
                    <div class="form-check px-0 w-20x">
                        <h5>Value</h5>
                        <input data-type="m" class="form-check-input" type="radio" id="radio1" disabled>
                        <label for="radio1"><input name="QuestionGroup.Questions[${ questionNumber - 1 }].Answers[${ answerIndex }].Value" type="text" class="form-control" /></label>
                    </div>
                    ${ getRemoveButton(`${ targetQuestionAnswer }-${ answerIndex }`, targetQuestionAnswer) }
                </div>
            </div>`;
};

var MultipleChoiceTemplate = function(targetQuestionAnswer, questionNumber, answers) {

    var answerIndex = getAnswerIndex(answers);
    
    return `<div id="question-${ questionNumber }-answer-${ answerIndex }"  class="answer col-12 pl-0">
                <div class="form-controler-input d-flex justify-conent-between">
                    <div class="form-check px-0 w-40x">
                        <h5>English Answer</h5>
                        <input data-type="m" class="form-check-input" type="checkbox" id="checkbox1" disabled>
                        <label for="checkbox1"><input name="QuestionGroup.Questions[${ questionNumber - 1 }].Answers[${ answerIndex }].AnswerEn" type="text" class="form-control" /></label>
                    </div>
                    <div class="form-check px-0 w-40x">
                        <h5>Thai Answer</h5>
                        <input data-type="m" class="form-check-input" type="checkbox" id="checkbox1" disabled>
                        <label for="checkbox1"><input name="QuestionGroup.Questions[${ questionNumber - 1 }].Answers[${ answerIndex }].AnswerTn" type="text" class="form-control" /></label>
                    </div>
                    <div class="form-check px-0 w-20x">
                        <h5>Value</h5>
                        <input data-type="m" class="form-check-input" type="checkbox" id="checkbox1" disabled>
                        <label for="checkbox1"><input name="QuestionGroup.Questions[${ questionNumber - 1 }].Answers[${ answerIndex }].Value" type="text" class="form-control" /></label>
                    </div>
                    ${ getRemoveButton(`${ targetQuestionAnswer }-${ answerIndex }`, targetQuestionAnswer) }
                </div>
            </div>`;
};

var ShortAnswerTemplate = function(targetQuestionAnswer, questionNumber, answers) {

    var answerIndex = getAnswerIndex(answers);

    return ` <div id="question-${ questionNumber }-${ answerIndex }" class="answer col-12 pl-0">
                <div class="form-controler-input d-flex justify-conent-between">
                    <div class="form-group w-40x">
                        <div class="form-controler-input">
                            <input data-type="s" name="QuestionGroup.Questions[${ questionNumber - 1 }].Answers[${ answerIndex }].AnswerEn" type="text" class="form-control height-40 pt-0" readonly placeholder="short answer text" />
                        </div>
                    </div>
                </div>
             </div>`;
};

var getRemoveButton = function(target, parent) {
    return  `<button type="button" class="btn btn--danger btn--circle js-delete-option" data-target="${ target }" data-parent="${ parent }"><i class="la la-times" data-target="${ target }" data-parent="${ parent }"></i></button>`;
};

var getAnswerIndex = function(answers) {
    if(answers === undefined || answers == '') { return 0 ; }
    return $(answers).length;
};

$('.questionnaire-body').on('change', '.js-select-question-type', function(e) {
    
    var questionNumber = $(e.target).data('question-number');
    var questionAnswer = $(e.target).data('target');
    var answerWrapper = $(questionAnswer).find('.answer-wrapper');
    $(answerWrapper).empty();

    var answers = $(answerWrapper).find('.answer');

    switch ($(e.target).val()) {
        case QuestionType.Ranking:
            $($(questionAnswer).find('.js-add-option')).data('type', QuestionType.Ranking);
            $($(questionAnswer).find('.js-add-option')).removeClass('d-none');
            
            return $(answerWrapper).html(RankingTemplate(questionAnswer, questionNumber, answers)); 
        case QuestionType.OneChoice:
            $($(questionAnswer).find('.js-add-option')).data('type', QuestionType.OneChoice);
            $($(questionAnswer).find('.js-add-option')).removeClass('d-none');
        
            return $(answerWrapper).html(OneChoiceTemplate(questionAnswer, questionNumber, answers)); 
        case QuestionType.MultipleChoice:
            $($(questionAnswer).find('.js-add-option')).data('type', QuestionType.MultipleChoice);
            $($(questionAnswer).find('.js-add-option')).removeClass('d-none');
           
            return $(answerWrapper).html(MultipleChoiceTemplate(questionAnswer, questionNumber, answers)); 
        case QuestionType.ShortAnswer:
            $($(questionAnswer).find('.js-add-option')).data('type', 's');
            $($(questionAnswer).find('.js-add-option')).addClass('d-none');
            
            return (answerWrapper).html(ShortAnswerTemplate(questionAnswer, questionNumber, answers)); 
    }
});

// Start Add Option
$('.questionnaire-body').on('click', '.js-add-option', function(e) {
    var questionType = $(e.target).data('type');
    var questionNumber = $(e.target).data('question-number');
    var questionAnswer = $(e.target).data('target');

    var answerWrapper = $(questionAnswer).find('.answer-wrapper');
    var answers = $(answerWrapper).find('.answer');

    switch (questionType) {
        case QuestionType.Ranking:
            return $(answerWrapper).append(RankingTemplate(questionAnswer, questionNumber, answers));
        case QuestionType.OneChoice:
            return $(answerWrapper).append(OneChoiceTemplate(questionAnswer, questionNumber, answers)); 
        case QuestionType.MultipleChoice:        
            return $(answerWrapper).append(MultipleChoiceTemplate(questionAnswer, questionNumber, answers)); 
        case QuestionType.ShortAnswer:
            return (answerWrapper).append(ShortAnswerTemplate(questionAnswer, questionNumber, answers));
    }
});
// End Add Option

// Start Delete Option
$('.questionnaire-body').on('click', '.js-delete-option', function(e) {
    
    var answers = $($(e.target).data('parent')).find('.answer-wrapper .answer');

    if(answers.length === 0) {
        Alert.renderAlert('Unable to delete answer', 'At least one answer is required', 'warning');
    } else {
        var questionAnswer = $(e.target).data('target');
        $(questionAnswer).remove();
    
        $(answers).map(function(index, item) {

            //Replace last occurrence of number with * using regular expression in Javascript: /\d+(?=\.[^.]+$)/

            $($(item).find(":input")).each(function () {
                if ($(this).attr("name")) {
                    $(this).attr("name", $(this).attr("name").replace(/\d+(?=\.[^.]+$)/, index));
                }
            });
        });
    }

    return answers;
});
//End Delete Option

$(function(){
    $('.js-select-question-type').each(function(){
        var $this = $(this);
        var options = $this.find('option');
        
        options.map((index, item) => {

            if ($this.data('value') === $(item).val()) {
                $(item).attr('selected', true);
            }
        });
    })
});