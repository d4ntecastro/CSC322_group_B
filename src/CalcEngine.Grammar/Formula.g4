// =============================================================================
// Formula.g4 — Formal grammar for the calculation engine's formula language.
//
// Supports: numbers, quoted text, cell references (B2), ranges (B2:B45),
// arithmetic (+ - * / ^), comparison (= <> < > <= >=), parentheses, and
// function calls (SUM, AVERAGE, MIN, MAX, COUNT, IF, ROUND, LOOKUP, ...).
//
// This is the single source of truth for what counts as a valid formula.
// The ANTLR toolchain generates FormulaLexer, FormulaParser, and
// FormulaBaseVisitor<T> from this file at build time.
// =============================================================================
grammar Formula;

// ---- Parser rules -----------------------------------------------------------

// Entry point: every formula starts with '=' (spreadsheet convention),
// followed by one expression, followed by end-of-input.
formula
    : '=' expr EOF
    ;

// Expression rules, ordered from tightest to loosest binding.
// ANTLR4 resolves precedence in a left-recursive rule by alternative order:
// alternatives listed first bind tighter than those listed later.
expr
    : '(' expr ')'                                  # ParenExpr
    | '-' expr                                      # UnaryMinusExpr
    | expr op='^' expr                               # PowerExpr
    | expr op=('*' | '/') expr                       # MulDivExpr
    | expr op=('+' | '-') expr                       # AddSubExpr
    | expr op=('=' | '<>' | '<=' | '>=' | '<' | '>') expr   # ComparisonExpr
    | IDENTIFIER '(' argList? ')'                    # FunctionCallExpr
    | CELL_RANGE                                     # RangeExpr
    | CELL_REF                                       # CellRefExpr
    | NUMBER                                         # NumberExpr
    | STRING                                         # StringExpr
    ;

// Comma-separated argument list for function calls, e.g. IF(B2>10, "high", "low")
argList
    : expr (',' expr)*
    ;

// ---- Lexer rules --------------------------------------------------------------
// Order matters: ANTLR tries rules top-to-bottom and picks the longest match,
// so CELL_RANGE must be declared before CELL_REF or "B2:B45" would lex as
// three separate tokens (CELL_REF, an unmatched ':', CELL_REF).

CELL_RANGE
    : CELL_REF ':' CELL_REF
    ;

CELL_REF
    : [A-Za-z]+ [0-9]+
    ;

NUMBER
    : [0-9]+ ('.' [0-9]+)?
    ;

STRING
    : '"' (~["\r\n])* '"'
    ;

IDENTIFIER
    : [A-Za-z_][A-Za-z0-9_]*
    ;

WS
    : [ \t\r\n]+ -> skip
    ;
