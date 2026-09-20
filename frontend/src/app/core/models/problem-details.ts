/** RFC 7807 error shape returned by every failing API call. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  /** Present on 400 responses: field name (camelCase) to the messages for that field. */
  errors?: Record<string, string[]>;
}
