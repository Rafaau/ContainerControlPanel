// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: opentelemetry/proto/collector/trace/v1/trace_service.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace OpenTelemetry.Proto.Collector.Trace.V1 {

  /// <summary>Holder for reflection information generated from opentelemetry/proto/collector/trace/v1/trace_service.proto</summary>
  public static partial class TraceServiceReflection {

    #region Descriptor
    /// <summary>File descriptor for opentelemetry/proto/collector/trace/v1/trace_service.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TraceServiceReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CjpvcGVudGVsZW1ldHJ5L3Byb3RvL2NvbGxlY3Rvci90cmFjZS92MS90cmFj",
            "ZV9zZXJ2aWNlLnByb3RvEiZvcGVudGVsZW1ldHJ5LnByb3RvLmNvbGxlY3Rv",
            "ci50cmFjZS52MRoob3BlbnRlbGVtZXRyeS9wcm90by90cmFjZS92MS90cmFj",
            "ZS5wcm90byJgChlFeHBvcnRUcmFjZVNlcnZpY2VSZXF1ZXN0EkMKDnJlc291",
            "cmNlX3NwYW5zGAEgAygLMisub3BlbnRlbGVtZXRyeS5wcm90by50cmFjZS52",
            "MS5SZXNvdXJjZVNwYW5zIngKGkV4cG9ydFRyYWNlU2VydmljZVJlc3BvbnNl",
            "EloKD3BhcnRpYWxfc3VjY2VzcxgBIAEoCzJBLm9wZW50ZWxlbWV0cnkucHJv",
            "dG8uY29sbGVjdG9yLnRyYWNlLnYxLkV4cG9ydFRyYWNlUGFydGlhbFN1Y2Nl",
            "c3MiSgoZRXhwb3J0VHJhY2VQYXJ0aWFsU3VjY2VzcxIWCg5yZWplY3RlZF9z",
            "cGFucxgBIAEoAxIVCg1lcnJvcl9tZXNzYWdlGAIgASgJMqIBCgxUcmFjZVNl",
            "cnZpY2USkQEKBkV4cG9ydBJBLm9wZW50ZWxlbWV0cnkucHJvdG8uY29sbGVj",
            "dG9yLnRyYWNlLnYxLkV4cG9ydFRyYWNlU2VydmljZVJlcXVlc3QaQi5vcGVu",
            "dGVsZW1ldHJ5LnByb3RvLmNvbGxlY3Rvci50cmFjZS52MS5FeHBvcnRUcmFj",
            "ZVNlcnZpY2VSZXNwb25zZSIAQpwBCilpby5vcGVudGVsZW1ldHJ5LnByb3Rv",
            "LmNvbGxlY3Rvci50cmFjZS52MUIRVHJhY2VTZXJ2aWNlUHJvdG9QAVoxZ28u",
            "b3BlbnRlbGVtZXRyeS5pby9wcm90by9vdGxwL2NvbGxlY3Rvci90cmFjZS92",
            "MaoCJk9wZW5UZWxlbWV0cnkuUHJvdG8uQ29sbGVjdG9yLlRyYWNlLlYxYgZw",
            "cm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::OpenTelemetry.Proto.Trace.V1.TraceReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::OpenTelemetry.Proto.Collector.Trace.V1.ExportTraceServiceRequest), global::OpenTelemetry.Proto.Collector.Trace.V1.ExportTraceServiceRequest.Parser, new[]{ "ResourceSpans" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::OpenTelemetry.Proto.Collector.Trace.V1.ExportTraceServiceResponse), global::OpenTelemetry.Proto.Collector.Trace.V1.ExportTraceServiceResponse.Parser, new[]{ "PartialSuccess" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::OpenTelemetry.Proto.Collector.Trace.V1.ExportTracePartialSuccess), global::OpenTelemetry.Proto.Collector.Trace.V1.ExportTracePartialSuccess.Parser, new[]{ "RejectedSpans", "ErrorMessage" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class ExportTraceServiceRequest : pb::IMessage<ExportTraceServiceRequest>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    public static readonly pb::MessageParser<ExportTraceServiceRequest> _parser = new pb::MessageParser<ExportTraceServiceRequest>(() => new ExportTraceServiceRequest());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<ExportTraceServiceRequest> Parser { get { return _parser; } }
    public pb::MessageParser<ExportTraceServiceRequest> Parser2 { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::OpenTelemetry.Proto.Collector.Trace.V1.TraceServiceReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ExportTraceServiceRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ExportTraceServiceRequest(ExportTraceServiceRequest other) : this() {
      resourceSpans_ = other.resourceSpans_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ExportTraceServiceRequest Clone() {
      return new ExportTraceServiceRequest(this);
    }

    /// <summary>Field number for the "resource_spans" field.</summary>
    public const int ResourceSpansFieldNumber = 1;
    private static readonly pb::FieldCodec<global::OpenTelemetry.Proto.Trace.V1.ResourceSpans> _repeated_resourceSpans_codec
        = pb::FieldCodec.ForMessage(10, global::OpenTelemetry.Proto.Trace.V1.ResourceSpans.Parser);
    private readonly pbc::RepeatedField<global::OpenTelemetry.Proto.Trace.V1.ResourceSpans> resourceSpans_ = new pbc::RepeatedField<global::OpenTelemetry.Proto.Trace.V1.ResourceSpans>();
    /// <summary>
    /// An array of ResourceSpans.
    /// For data coming from a single resource this array will typically contain one
    /// element. Intermediary nodes (such as OpenTelemetry Collector) that receive
    /// data from multiple origins typically batch the data before forwarding further and
    /// in that case this array will contain multiple elements.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::OpenTelemetry.Proto.Trace.V1.ResourceSpans> ResourceSpans {
      get { return resourceSpans_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as ExportTraceServiceRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(ExportTraceServiceRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!resourceSpans_.Equals(other.resourceSpans_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= resourceSpans_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      resourceSpans_.WriteTo(output, _repeated_resourceSpans_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      resourceSpans_.WriteTo(ref output, _repeated_resourceSpans_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      size += resourceSpans_.CalculateSize(_repeated_resourceSpans_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(ExportTraceServiceRequest other) {
      if (other == null) {
        return;
      }
      resourceSpans_.Add(other.resourceSpans_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            resourceSpans_.AddEntriesFrom(input, _repeated_resourceSpans_codec);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            resourceSpans_.AddEntriesFrom(ref input, _repeated_resourceSpans_codec);
            break;
          }
        }
      }
    }
    #endif

  }

  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class ExportTraceServiceResponse : pb::IMessage<ExportTraceServiceResponse>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<ExportTraceServiceResponse> _parser = new pb::MessageParser<ExportTraceServiceResponse>(() => new ExportTraceServiceResponse());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<ExportTraceServiceResponse> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::OpenTelemetry.Proto.Collector.Trace.V1.TraceServiceReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ExportTraceServiceResponse() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ExportTraceServiceResponse(ExportTraceServiceResponse other) : this() {
      partialSuccess_ = other.partialSuccess_ != null ? other.partialSuccess_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ExportTraceServiceResponse Clone() {
      return new ExportTraceServiceResponse(this);
    }

    /// <summary>Field number for the "partial_success" field.</summary>
    public const int PartialSuccessFieldNumber = 1;
    private global::OpenTelemetry.Proto.Collector.Trace.V1.ExportTracePartialSuccess partialSuccess_;
    /// <summary>
    /// The details of a partially successful export request.
    ///
    /// If the request is only partially accepted
    /// (i.e. when the server accepts only parts of the data and rejects the rest)
    /// the server MUST initialize the `partial_success` field and MUST
    /// set the `rejected_&lt;signal>` with the number of items it rejected.
    ///
    /// Servers MAY also make use of the `partial_success` field to convey
    /// warnings/suggestions to senders even when the request was fully accepted.
    /// In such cases, the `rejected_&lt;signal>` MUST have a value of `0` and
    /// the `error_message` MUST be non-empty.
    ///
    /// A `partial_success` message with an empty value (rejected_&lt;signal> = 0 and
    /// `error_message` = "") is equivalent to it not being set/present. Senders
    /// SHOULD interpret it the same way as in the full success case.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::OpenTelemetry.Proto.Collector.Trace.V1.ExportTracePartialSuccess PartialSuccess {
      get { return partialSuccess_; }
      set {
        partialSuccess_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as ExportTraceServiceResponse);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(ExportTraceServiceResponse other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(PartialSuccess, other.PartialSuccess)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (partialSuccess_ != null) hash ^= PartialSuccess.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (partialSuccess_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(PartialSuccess);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (partialSuccess_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(PartialSuccess);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (partialSuccess_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(PartialSuccess);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(ExportTraceServiceResponse other) {
      if (other == null) {
        return;
      }
      if (other.partialSuccess_ != null) {
        if (partialSuccess_ == null) {
          PartialSuccess = new global::OpenTelemetry.Proto.Collector.Trace.V1.ExportTracePartialSuccess();
        }
        PartialSuccess.MergeFrom(other.PartialSuccess);
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (partialSuccess_ == null) {
              PartialSuccess = new global::OpenTelemetry.Proto.Collector.Trace.V1.ExportTracePartialSuccess();
            }
            input.ReadMessage(PartialSuccess);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            if (partialSuccess_ == null) {
              PartialSuccess = new global::OpenTelemetry.Proto.Collector.Trace.V1.ExportTracePartialSuccess();
            }
            input.ReadMessage(PartialSuccess);
            break;
          }
        }
      }
    }
    #endif

  }

  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class ExportTracePartialSuccess : pb::IMessage<ExportTracePartialSuccess>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<ExportTracePartialSuccess> _parser = new pb::MessageParser<ExportTracePartialSuccess>(() => new ExportTracePartialSuccess());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<ExportTracePartialSuccess> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::OpenTelemetry.Proto.Collector.Trace.V1.TraceServiceReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ExportTracePartialSuccess() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ExportTracePartialSuccess(ExportTracePartialSuccess other) : this() {
      rejectedSpans_ = other.rejectedSpans_;
      errorMessage_ = other.errorMessage_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ExportTracePartialSuccess Clone() {
      return new ExportTracePartialSuccess(this);
    }

    /// <summary>Field number for the "rejected_spans" field.</summary>
    public const int RejectedSpansFieldNumber = 1;
    private long rejectedSpans_;
    /// <summary>
    /// The number of rejected spans.
    ///
    /// A `rejected_&lt;signal>` field holding a `0` value indicates that the
    /// request was fully accepted.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long RejectedSpans {
      get { return rejectedSpans_; }
      set {
        rejectedSpans_ = value;
      }
    }

    /// <summary>Field number for the "error_message" field.</summary>
    public const int ErrorMessageFieldNumber = 2;
    private string errorMessage_ = "";
    /// <summary>
    /// A developer-facing human-readable message in English. It should be used
    /// either to explain why the server rejected parts of the data during a partial
    /// success or to convey warnings/suggestions during a full success. The message
    /// should offer guidance on how users can address such issues.
    ///
    /// error_message is an optional field. An error_message with an empty value
    /// is equivalent to it not being set.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string ErrorMessage {
      get { return errorMessage_; }
      set {
        errorMessage_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as ExportTracePartialSuccess);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(ExportTracePartialSuccess other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (RejectedSpans != other.RejectedSpans) return false;
      if (ErrorMessage != other.ErrorMessage) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (RejectedSpans != 0L) hash ^= RejectedSpans.GetHashCode();
      if (ErrorMessage.Length != 0) hash ^= ErrorMessage.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (RejectedSpans != 0L) {
        output.WriteRawTag(8);
        output.WriteInt64(RejectedSpans);
      }
      if (ErrorMessage.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(ErrorMessage);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (RejectedSpans != 0L) {
        output.WriteRawTag(8);
        output.WriteInt64(RejectedSpans);
      }
      if (ErrorMessage.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(ErrorMessage);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (RejectedSpans != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(RejectedSpans);
      }
      if (ErrorMessage.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ErrorMessage);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(ExportTracePartialSuccess other) {
      if (other == null) {
        return;
      }
      if (other.RejectedSpans != 0L) {
        RejectedSpans = other.RejectedSpans;
      }
      if (other.ErrorMessage.Length != 0) {
        ErrorMessage = other.ErrorMessage;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            RejectedSpans = input.ReadInt64();
            break;
          }
          case 18: {
            ErrorMessage = input.ReadString();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            RejectedSpans = input.ReadInt64();
            break;
          }
          case 18: {
            ErrorMessage = input.ReadString();
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
